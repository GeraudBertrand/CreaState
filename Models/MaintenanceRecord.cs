using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class MaintenanceRecord
    {
        [Key]
        public int Id { get; set; }

        // FK vers Printer
        [Required]
        public string PrinterId { get; set; } = string.Empty;
        public Printer? Printer { get; set; }

        // FK vers Member (qui a effectué la maintenance)
        public int PerformedByMemberId { get; set; }
        public Member? PerformedBy { get; set; }

        public MaintenanceType Type { get; set; } = MaintenanceType.Scheduled;

        public string Description { get; set; } = string.Empty;

        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        public DateTime? NextScheduledAt { get; set; }

        public bool IsBreakdownReport { get; set; } = false;

        public bool IsResolved { get; set; } = false;
        public DateTime? ResolvedAt { get; set; }
        public int? ResolvedByMemberId { get; set; }
        public Member? ResolvedBy { get; set; }
    }
}
