using CreaState.Repositories.Interfaces;
using MQTTnet;
using MQTTnet.Formatter;
using System.Text;

namespace CreaState.Services
{
    public class PrinterMqttWorker : BackgroundService
    {
        private readonly PrinterService _printerService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PrinterMqttWorker> _logger;

        public PrinterMqttWorker(PrinterService printerService, IServiceScopeFactory scopeFactory, ILogger<PrinterMqttWorker> logger)
        {
            _printerService = printerService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Démarrage du service MQTT Bambu Lab...");

            // Une boucle de connexion par imprimante, relancée quand sa config change en base (ajout/modif/retrait depuis l'admin)
            var running = new Dictionary<int, (Models.Printer Config, CancellationTokenSource Cts)>();
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        List<Models.Printer> dbPrinters;
                        using (var scope = _scopeFactory.CreateScope())
                            dbPrinters = await scope.ServiceProvider.GetRequiredService<IPrinterRepository>().GetEnabledAsync();

                        _printerService.LoadPrintersFromDb(dbPrinters);

                        foreach (var (id, run) in running.Where(r => !dbPrinters.Any(p => SameConnection(p, r.Value.Config))).ToList())
                        {
                            run.Cts.Cancel();
                            running.Remove(id);
                        }

                        foreach (var printer in dbPrinters.Where(p => !running.ContainsKey(p.Id)))
                        {
                            // Les données de la base (avec AccessCode), pas PrinterService.GetPrinters() qui le vide
                            var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                            running[printer.Id] = (printer, cts);
                            _ = ConnectAndListenToPrinter(printer, cts.Token);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogWarning("Lecture des imprimantes impossible : {Message}", ex.Message);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                foreach (var run in running.Values) run.Cts.Cancel();
            }
        }

        private static bool SameConnection(Models.Printer a, Models.Printer b)
            => a.Id == b.Id && a.IpAddress == b.IpAddress && a.SerialNumber == b.SerialNumber && a.AccessCode == b.AccessCode;

        private async Task ConnectAndListenToPrinter(Models.Printer printer, CancellationToken token)
        {
            if (string.IsNullOrEmpty(printer.IpAddress) || string.IsNullOrEmpty(printer.SerialNumber))
            {
                _logger.LogWarning($"Impossible de démarrer MQTT pour {printer.Name} : IP ou Serial manquant.");
                return;
            }

            try
            {
                await ListenLoop(printer, token);
            }
            catch (OperationCanceledException) { }
            finally
            {
                _printerService.SetOffline(printer.IpAddress);
            }
        }

        private async Task ListenLoop(Models.Printer printer, CancellationToken token)
        {
            var mqttFactory = new MqttClientFactory();
            using var mqttClient = mqttFactory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(printer.IpAddress, 8883)
                .WithCredentials("bblp", printer.AccessCode)
                .WithClientId($"Crealab_{printer.SerialNumber}_{Guid.NewGuid().ToString().Substring(0, 4)}")
                .WithProtocolVersion(MqttProtocolVersion.V311)
                .WithTlsOptions(o =>
                {
                    o.UseTls(true);
                    o.WithCertificateValidationHandler(_ => true);
                })
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(10))
                .WithCleanSession(true)
                .Build();

            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                _printerService.UpdateFromMqtt(printer.IpAddress, payload);
                return Task.CompletedTask;
            };

            // Imprimante éteinte (la nuit) : on la marque hors ligne, la boucle ci-dessous la retrouve à son rallumage
            mqttClient.DisconnectedAsync += e =>
            {
                if (e.ClientWasConnected)
                    _logger.LogInformation($"Connexion perdue avec {printer.Name}, nouvelle tentative régulière.");
                _printerService.SetOffline(printer.IpAddress);
                return Task.CompletedTask;
            };

            var failures = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!mqttClient.IsConnected)
                    {
                        using (var connectTimeout = CancellationTokenSource.CreateLinkedTokenSource(token))
                        {
                            connectTimeout.CancelAfter(TimeSpan.FromSeconds(5));
                            await mqttClient.ConnectAsync(options, connectTimeout.Token);
                        }

                        string reportTopic = $"device/{printer.SerialNumber}/report";
                        await mqttClient.SubscribeAsync(reportTopic, cancellationToken: token);

                        _logger.LogInformation($"Connecté à {printer.Name} !");
                        failures = 0;

                        _ = Task.Run(() => MaintainPushAllLoop(mqttClient, printer.SerialNumber, token), token);
                    }
                }
                catch (Exception ex) when (!token.IsCancellationRequested)
                {
                    // Un seul warning par coupure, sinon une imprimante éteinte remplit les logs toute la nuit
                    if (failures++ == 0)
                        _logger.LogWarning($"{printer.Name} injoignable ({ex.Message}), nouvelle tentative régulière.");
                }

                // 10 s quand tout va bien, jusqu'à 60 s entre deux essais quand l'imprimante reste éteinte
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(10 * Math.Max(failures, 1), 60)), token);
            }
        }

        private async Task MaintainPushAllLoop(IMqttClient client, string serialNumber, CancellationToken token)
        {
            string requestTopic = $"device/{serialNumber}/request";
            string pushAllPayload = "{\"pushing\": {\"sequence_id\": \"0\", \"command\": \"pushall\"}}";

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(requestTopic)
                .WithPayload(pushAllPayload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            while (client.IsConnected && !token.IsCancellationRequested)
            {
                try
                {
                    await client.PublishAsync(message, token);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erreur PushAll: {ex.Message}");
                    break;
                }

                try { await Task.Delay(TimeSpan.FromMinutes(5), token); }
                catch (OperationCanceledException) { break; }
            }
        }
    }
}
