using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreaState.Models
{
    public class PrintJob
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string FileName { get; set; } = string.Empty;

        // FK vers Printer
        [Required]
        public string PrinterId { get; set; } = string.Empty;
        public Printer? Printer { get; set; }

        // FK vers Member (nullable — les prints MQTT n'ont pas d'utilisateur connu)
        public int? RequestedByUserId { get; set; }
        public Member? RequestedBy { get; set; }

        // FK optionnelle vers Request
        public int? RequestId { get; set; }
        public Request? Request { get; set; }

        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }

        public PrintStatus Status { get; set; }

        public int FilamentWeightGrams { get; set; }

        // Propriétés calculées pour compatibilité avec les vues existantes
        [NotMapped]
        public string PrinterName => Printer?.Name ?? PrinterId;

        [NotMapped]
        public string StatusLabel => Status.GetDisplayName();

        [NotMapped]
        public DateTime EndTime => StartTime.Add(Duration);

        [NotMapped]
        public string DurationLabel => $"{(int)Duration.TotalHours}h {Duration.Minutes:00}m";

        [NotMapped]
        public string StatusCssClass => Status switch
        {
            PrintStatus.Success => "status-success",
            PrintStatus.Failed => "status-failed",
            PrintStatus.Cancelled => "status-cancelled",
            _ => ""
        };
    }
}
