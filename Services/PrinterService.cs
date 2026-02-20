using CreaState.Models;
using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace CreaState.Services
{
    public class PrinterService
    {
        // ConcurrentDictionary est vital ici : plusieurs threads (MQTT + UI) vont lire/écrire en même temps.
        // La clé est l'adresse IP (unique par machine).
        private readonly ConcurrentDictionary<string, Printer> _printers = new();

        // L'événement que Blazor écoutera pour rafraîchir la page
        public event Action? OnChange;

        public PrinterService()
        {
            // Initialisation avec des données de base (ou chargement depuis la DB / Config)
            // Dans le futur, tu chargeras ça depuis appsettings.json ou SQL
            var p1 = new Printer { Name = "Ratome", IpAddress = "10.3.212.16", Model = "A1 Mini", AccessCode = "26110863", SerialNumber = "0309DA3C3100431" };
            var p2 = new Printer { Name = "Bonnie", IpAddress = "10.3.212.10", Model = "A1 Mini", AccessCode = "87654321", SerialNumber = "0309DA3C3100060" };
            var p3 = new Printer { Name = "Hubble", IpAddress = "10.3.212.19", Model = "A1 Mini", AccessCode = "84466330", SerialNumber = "0309DA422200342" };
            var p4 = new Printer { Name = "R2-D2", IpAddress = "10.3.212.23", Model = "A1 Mini", AccessCode = "85255827", SerialNumber = "0309DA441501115" };

            _printers.TryAdd(p1.IpAddress, p1);
            _printers.TryAdd(p2.IpAddress, p2);
            _printers.TryAdd(p3.IpAddress, p3);
            _printers.TryAdd(p4.IpAddress, p4);
        }

        // Récupérer la liste pour l'affichage
        public List<Printer> GetPrinters() => [.. _printers.Values.OrderBy(p => p.Name)];

        // C'est ICI que la magie opère. Le Listener MQTT appelle cette méthode.
        public void UpdateFromMqtt(string ipAddress, string jsonPayload)
        {
            if (!_printers.TryGetValue(ipAddress, out var printer)) return;

            try
            {
                // Parsing dynamique du JSON Bambu Lab
                var node = JsonNode.Parse(jsonPayload);
                var printData = node?["print"]; // Bambu met tout dans un objet "print"

                if (printData != null)
                {
                    // 1. Mise à jour du pourcentage
                    if (printData["mc_percent"] != null)
                    {
                        printer.Progress = ParseBambuNumber(printData["mc_percent"]);
                    }

                    // 2. Mise à jour des températures
                    if (printData["nozzle_temper"] != null)
                    {
                        printer.NozzleTemp = ParseBambuNumber(printData["nozzle_temper"]);
                    }
                    if (printData["bed_temper"] != null)
                    {
                        printer.BedTemp = ParseBambuNumber(printData["bed_temper"]);
                    }

                    // 3. Mise à jour du statut (Running, Idle, etc.)
                    if (printData["gcode_state"] != null)
                    {
                        string state = printData["gcode_state"]!.ToString();
                        printer.Status = MapBambuStateToEnum(state);
                    }

                    // 4. Temps restant (Bambu envoie des minutes restantes)
                    if (printData["mc_remaining_time"] != null)
                    {
                        printer.TimeRemainingMinutes = ParseBambuNumber(printData["mc_remaining_time"]);
                    }

                    // 5. Nom du fichier (souvent dans 'subtask_name')
                    if (printData["subtask_name"] != null)
                    {
                        printer.CurrentFile = printData["subtask_name"].ToString().Replace(".gcode", "");
                    }

                    // Notifier l'interface graphique qu'il y a du changement
                    NotifyStateChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur parsing JSON pour {ipAddress}: {ex.Message}");
            }
        }

        private int ParseBambuNumber(JsonNode? node)
        {
            if (node == null) return 0;

            // En forçant d'abord en string, puis en parsant en "double" avec l'Invariant Culture (pour le point décimal),
            // on s'assure de ne jamais crasher, que Bambu envoie "75", 75, ou 215.5.
            if (double.TryParse(node.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
            {
                return (int)Math.Round(result);
            }

            return 0; // Fallback par défaut si ce n'est vraiment pas un nombre
        }

        // Helper pour traduire le langage "Bambu" en langage "Crealab"
        private PrinterStatus MapBambuStateToEnum(string bambuState)
        {
            return bambuState switch
            {
                "RUNNING" => PrinterStatus.Printing,
                "IDLE" => PrinterStatus.Idle,
                "PAUSE" => PrinterStatus.Pause,
                "FINISH" => PrinterStatus.Success,
                "FAILED" => PrinterStatus.Error,
                _ => PrinterStatus.Idle
            };
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
