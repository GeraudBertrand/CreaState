namespace CreaState.DTOs.Printers
{
    public class PrinterStatusDto
    {
        public int PrinterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string CurrentFile { get; set; } = string.Empty;
        public int Progress { get; set; }
        public int TimeRemainingMinutes { get; set; }
        public int NozzleTemp { get; set; }
        public int BedTemp { get; set; }
        public string FilamentType { get; set; } = string.Empty;
        public string FilamentColor { get; set; } = string.Empty;
    }
}
