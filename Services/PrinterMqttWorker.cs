using MQTTnet;
using MQTTnet.Formatter;
using System.Text;

namespace CreaState.Services
{
    public class PrinterMqttWorker : BackgroundService
    {
        private readonly PrinterService _printerService;
        private readonly ILogger<PrinterMqttWorker> _logger;

        public PrinterMqttWorker(PrinterService printerService, ILogger<PrinterMqttWorker> logger)
        {
            _printerService = printerService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Démarrage du service MQTT Bambu Lab...");

            var printers = _printerService.GetPrinters();
            var tasks = new List<Task>();

            foreach (var printer in printers)
            {
                // On lance une tâche isolée pour chaque imprimante
                tasks.Add(ConnectAndListenToPrinter(printer, stoppingToken));
            }

            await Task.WhenAll(tasks);
        }

        private async Task ConnectAndListenToPrinter(Models.Printer printer, CancellationToken token)
        {
            // Vérification de sécurité avant de démarrer
            if (string.IsNullOrEmpty(printer.IpAddress) || string.IsNullOrEmpty(printer.SerialNumber))
            {
                _logger.LogWarning($"Impossible de démarrer MQTT pour {printer.Name} : IP ou Serial manquant.");
                return;
            }

            var mqttFactory = new MqttClientFactory();
            using var mqttClient = mqttFactory.CreateMqttClient();

            // Configuration Robuste (basée sur ton test réussi)
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(printer.IpAddress, 8883)
                .WithCredentials("bblp", printer.AccessCode)
                .WithClientId($"Crealab_{printer.SerialNumber}_{Guid.NewGuid().ToString().Substring(0, 4)}")
                .WithProtocolVersion(MqttProtocolVersion.V311) // FORCE V3.1.1
                .WithTlsOptions(o =>
                {
                    o.UseTls(true);
                    // Bypass total de la validation SSL
                    o.WithCertificateValidationHandler(_ => true);
                })
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(10))
                .WithCleanSession(true)
                .Build();

            // Réception des messages
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                // On passe le relais au PrinterService pour le parsing et la mise à jour UI
                _printerService.UpdateFromMqtt(printer.IpAddress, payload);

                return Task.CompletedTask;
            };

            // Boucle de connexion et de maintien
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!mqttClient.IsConnected)
                    {
                        _logger.LogInformation($"Connexion à {printer.Name} ({printer.IpAddress})...");

                        using (var connectTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                        {
                            await mqttClient.ConnectAsync(options, connectTimeout.Token);
                        }

                        // Abonnement au topic spécifique
                        string reportTopic = $"device/{printer.SerialNumber}/report";
                        await mqttClient.SubscribeAsync(reportTopic);

                        _logger.LogInformation($"Connecté à {printer.Name} !");

                        // Lancement de la boucle de commande "PushAll" en parallèle
                        // On ne l'attend pas (Fire and Forget) pour ne pas bloquer la boucle principale
                        _ = Task.Run(() => MaintainPushAllLoop(mqttClient, printer.SerialNumber, token), token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Échec connexion {printer.Name}: {ex.Message}. Retry 10s...");
                }

                await Task.Delay(10000, token); // Vérification toutes les 10s
            }
        }

        // Tâche secondaire : Envoie "PushAll" régulièrement pour rafraîchir toutes les données
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
                    // Envoi de la commande
                    await client.PublishAsync(message, token);
                    // _logger.LogInformation($"PushAll envoyé à {serialNumber}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erreur PushAll: {ex.Message}");
                    break; // Si erreur d'envoi, on sort de la boucle (le client principal tentera de reconnecter)
                }

                // Pause de 5 minutes entre chaque demande complète
                await Task.Delay(TimeSpan.FromMinutes(5), token);
            }
        }
    }
}
