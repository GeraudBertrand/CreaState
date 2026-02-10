namespace CreaState.Models
{
    public class PrintJob
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;

        public string PrinterName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }

        public PrintStatus Status { get; set; }
        public string StatusLabel => Status.GetDisplayName();

        public int FilamentWeightGrams { get; set; }

        public DateTime EndTime => StartTime.Add(Duration);

        public string DurationLabel => $"{(int)Duration.TotalHours}h {Duration.Minutes:00}m";

        public string StatusCssClass => Status switch
        {
            PrintStatus.Success => "status-success",
            PrintStatus.Failed => "status-failed",
            PrintStatus.Cancelled => "status-cancelled",
            _ => ""
        };
    }
}
