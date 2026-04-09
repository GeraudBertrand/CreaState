namespace CreaState.DTOs.Printers
{
    public class PrintJobDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public int PrinterId { get; set; }
        public string PrinterName { get; set; } = string.Empty;
        public int? RequeteId { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string DurationLabel { get; set; } = string.Empty;
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string StatusCssClass { get; set; } = string.Empty;
        public int FilamentWeightGrams { get; set; }
    }
}
