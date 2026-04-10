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

            using (var scope = _scopeFactory.CreateScope())
            {
                var printerRepo = scope.ServiceProvider.GetRequiredService<IPrinterRepository>();
                var dbPrinters = await printerRepo.GetEnabledAsync();
                _printerService.LoadPrintersFromDb(dbPrinters);
            }

            var printers = _printerService.GetPrinters();
            var tasks = new List<Task>();

            foreach (var printer in printers)
                tasks.Add(ConnectAndListenToPrinter(printer, stoppingToken));

            await Task.WhenAll(tasks);
        }

        private async Task ConnectAndListenToPrinter(Models.Printer printer, CancellationToken token)
        {
            if (string.IsNullOrEmpty(printer.IpAddress) || string.IsNullOrEmpty(printer.SerialNumber))
            {
                _logger.LogWarning($"Impossible de démarrer MQTT pour {printer.Name} : IP ou Serial manquant.");
                return;
            }

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

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!mqttClient.IsConnected)
                    {
                        _logger.LogInformation($"Connexion à {printer.Name} ({printer.IpAddress})...");

                        using (var connectTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                            await mqttClient.ConnectAsync(options, connectTimeout.Token);

                        string reportTopic = $"device/{printer.SerialNumber}/report";
                        await mqttClient.SubscribeAsync(reportTopic);

                        _logger.LogInformation($"Connecté à {printer.Name} !");

                        _ = Task.Run(() => MaintainPushAllLoop(mqttClient, printer.SerialNumber, token), token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Échec connexion {printer.Name}: {ex.Message}. Retry 10s...");
                }

                await Task.Delay(10000, token);
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

                await Task.Delay(TimeSpan.FromMinutes(5), token);
            }
        }
    }
}
