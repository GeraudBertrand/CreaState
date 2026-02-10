namespace CreaState.Models
{

    public class Printer
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Bambu Printer";
        public string Model { get; set; } = "A1"; // A1, X1C, P1S
        public string IpAddress { get; set; } = "192.168.1.xxx";
        public string AccessCode { get; set; } = "000000";

        public PrinterStatus Status { get; set; } = PrinterStatus.Offline;
        public string StatusLabel => Status.GetDisplayName();

        // Données d'impression
        public string CurrentFile { get; set; } = "";
        public int Progress { get; set; } = 0;
        public int TimeRemainingMinutes { get; set; } = 0;

        // Télémétrie
        public int NozzleTemp { get; set; } = 0;
        public int BedTemp { get; set; } = 0;
        public string FilamentType { get; set; } = "PLA";
        public string FilamentColor { get; set; } = "#ffffff"; // Code Hex pour l'UI

        // Helpers pour l'affichage
        public bool IsActive => Status == PrinterStatus.Printing || Status == PrinterStatus.Pause;

        public string GetStatusColor() => Status switch
        {
            PrinterStatus.Printing => "printing",
            PrinterStatus.Idle => "idle",
            PrinterStatus.Success => "success",
            PrinterStatus.Error => "error",
            _ => "offline"
        };
    }
}