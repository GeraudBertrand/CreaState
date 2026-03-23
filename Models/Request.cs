using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreaState.Models
{
    public class Request
    {
        [Key]
        public int Id { get; set; }

        public RequestType Type { get; set; } = RequestType.FDM;

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public RequestStatus Status { get; set; } = RequestStatus.Submitted;

        [MaxLength(200)]
        public string? MaterialPreference { get; set; }

        public string? RejectionReason { get; set; }

        // FK vers Member (demandeur)
        public int RequestedByMemberId { get; set; }
        public Member? RequestedBy { get; set; }

        // FK vers Member (responsable assigné, nullable)
        public int? AssignedToMemberId { get; set; }
        public Member? AssignedTo { get; set; }

        // FK vers Printer (machine assignée, nullable)
        public string? PrinterId { get; set; }
        public Printer? Printer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public bool NotificationSent { get; set; } = false;

        // Navigation
        public ICollection<RequestFile> Files { get; set; } = [];
        public ICollection<PrintJob> PrintJobs { get; set; } = [];
        public ICollection<RequestComment> Comments { get; set; } = [];

        [NotMapped]
        public string StatusLabel => Status.GetDisplayName();

        [NotMapped]
        public string TypeLabel => Type.GetDisplayName();

        [NotMapped]
        public string StatusCssClass => Status switch
        {
            RequestStatus.Submitted => "badge-info",
            RequestStatus.UnderReview => "badge-warning",
            RequestStatus.Approved => "badge-success",
            RequestStatus.Rejected => "badge-danger",
            RequestStatus.InProgress => "badge-primary",
            RequestStatus.Completed => "badge-success",
            RequestStatus.Cancelled => "badge-secondary",
            _ => ""
        };
    }
}
