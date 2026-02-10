using MQTTnet;
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

            // Récupérer la liste des imprimantes à écouter
            var printers = _printerService.GetPrinters();
            var tasks = new List<Task>();

            foreach (var printer in printers)
            {
                // Pour chaque imprimante, on lance une tâche de connexion dédiée
                tasks.Add(ConnectAndListenToPrinter(printer, stoppingToken));
            }

            // On attend que tout le monde ait fini (ne finira jamais tant que l'app tourne)
            await Task.WhenAll(tasks);
        }

        private async Task ConnectAndListenToPrinter(Models.Printer printer, CancellationToken token)
        {
            var mqttFactory = new MqttClientFactory();
            using var mqttClient = mqttFactory.CreateMqttClient();

            // Configuration SSL spécifique Bambu Lab
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(printer.IpAddress, 8883)
                .WithCredentials("bblp", printer.AccessCode)
                .WithTlsOptions(o =>
                {
                    o.WithAllowUntrustedCertificates(true);
                    o.WithIgnoreCertificateChainErrors(true);
                })
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(10))
                .Build();

            // Callback : Quand un message arrive
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                // 👉 On envoie la donnée au PrinterService
                _printerService.UpdateFromMqtt(printer.IpAddress, payload);

                return Task.CompletedTask;
            };

            // Boucle de reconnexion automatique (Résilience)
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!mqttClient.IsConnected)
                    {
                        _logger.LogInformation($"Connexion à {printer.Name} ({printer.IpAddress})...");
                        await mqttClient.ConnectAsync(options, token);

                        // S'abonner au flux de données
                        await mqttClient.SubscribeAsync("device/+/report");
                        _logger.LogInformation($"✅ Connecté à {printer.Name} !");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"❌ Échec connexion {printer.Name}: {ex.Message}. Nouvelle tentative dans 5s.");
                }

                // Pause de 5 secondes avant de vérifier la connexion
                await Task.Delay(5000, token);
            }
        }
    }
}
