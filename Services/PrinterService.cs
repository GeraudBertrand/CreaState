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
            var p1 = new Printer { Name = "Bambu A1 - 01", IpAddress = "192.168.1.50", Model = "A1", AccessCode = "12345678" };
            var p2 = new Printer { Name = "Bambu X1C", IpAddress = "192.168.1.60", Model = "X1C", AccessCode = "87654321" };

            _printers.TryAdd(p1.IpAddress, p1);
            _printers.TryAdd(p2.IpAddress, p2);
        }

        // Récupérer la liste pour l'affichage
        public List<Printer> GetPrinters() => _printers.Values.OrderBy(p => p.Name).ToList();

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
                        printer.Progress = (int)printData["mc_percent"];
                    }

                    // 2. Mise à jour des températures
                    if (printData["nozzle_temper"] != null)
                    {
                        printer.NozzleTemp = (int)printData["nozzle_temper"];
                    }
                    if (printData["bed_temper"] != null)
                    {
                        printer.BedTemp = (int)printData["bed_temper"];
                    }

                    // 3. Mise à jour du statut (Running, Idle, etc.)
                    if (printData["gcode_state"] != null)
                    {
                        string state = printData["gcode_state"].ToString();
                        printer.Status = MapBambuStateToEnum(state);
                    }

                    // 4. Temps restant (Bambu envoie des minutes restantes)
                    if (printData["mc_remaining_time"] != null)
                    {
                        printer.TimeRemainingMinutes = (int)printData["mc_remaining_time"];
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
