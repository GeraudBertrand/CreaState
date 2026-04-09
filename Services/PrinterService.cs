using CreaState.Models;
using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace CreaState.Services
{
    public class PrinterRuntimeState
    {
        public int PrinterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public PrinterStatus Status { get; set; } = PrinterStatus.Offline;
        public string CurrentFile { get; set; } = string.Empty;
        public int Progress { get; set; }
        public int TimeRemainingMinutes { get; set; }
        public int NozzleTemp { get; set; }
        public int BedTemp { get; set; }
        public string FilamentType { get; set; } = "PLA";
        public string FilamentColor { get; set; } = "#ffffff";
    }

    public class PrinterService
    {
        private readonly ConcurrentDictionary<string, PrinterRuntimeState> _printers = new();

        public event Action? OnChange;

        public void LoadPrintersFromDb(List<Printer> dbPrinters)
        {
            _printers.Clear();
            foreach (var p in dbPrinters)
            {
                _printers.TryAdd(p.IpAddress, new PrinterRuntimeState
                {
                    PrinterId = p.Id,
                    Name = p.Name,
                    IpAddress = p.IpAddress,
                    Model = p.Model,
                    SerialNumber = p.SerialNumber
                });
            }
        }

        public List<Printer> GetPrinters()
        {
            // Return Printer models for MQTT worker compatibility
            return _printers.Values.Select(s => new Printer
            {
                Id = s.PrinterId,
                Name = s.Name,
                IpAddress = s.IpAddress,
                Model = s.Model,
                SerialNumber = s.SerialNumber,
                AccessCode = string.Empty // Not stored in runtime state
            }).OrderBy(p => p.Name).ToList();
        }

        public List<PrinterRuntimeState> GetPrinterStates()
            => [.. _printers.Values.OrderBy(p => p.Name)];

        public void UpdateFromMqtt(string ipAddress, string jsonPayload)
        {
            if (!_printers.TryGetValue(ipAddress, out var state)) return;

            try
            {
                var node = JsonNode.Parse(jsonPayload);
                var printData = node?["print"];

                if (printData != null)
                {
                    if (printData["mc_percent"] != null)
                        state.Progress = ParseBambuNumber(printData["mc_percent"]);

                    if (printData["nozzle_temper"] != null)
                        state.NozzleTemp = ParseBambuNumber(printData["nozzle_temper"]);

                    if (printData["bed_temper"] != null)
                        state.BedTemp = ParseBambuNumber(printData["bed_temper"]);

                    if (printData["gcode_state"] != null)
                        state.Status = MapBambuStateToEnum(printData["gcode_state"]!.ToString());

                    if (printData["mc_remaining_time"] != null)
                        state.TimeRemainingMinutes = ParseBambuNumber(printData["mc_remaining_time"]);

                    if (printData["subtask_name"] != null)
                        state.CurrentFile = printData["subtask_name"].ToString().Replace(".gcode", "");

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
            if (double.TryParse(node.ToString(), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double result))
                return (int)Math.Round(result);
            return 0;
        }

        private PrinterStatus MapBambuStateToEnum(string bambuState) => bambuState switch
        {
            "RUNNING" => PrinterStatus.Printing,
            "IDLE" => PrinterStatus.Idle,
            "PAUSE" => PrinterStatus.Pause,
            "FINISH" => PrinterStatus.Success,
            "FAILED" => PrinterStatus.Error,
            _ => PrinterStatus.Idle
        };

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
