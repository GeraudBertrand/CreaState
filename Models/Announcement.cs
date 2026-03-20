using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreaState.Models
{
    public enum AnnouncementSeverity
    {
        Info,
        Warning,
        Danger
    }

    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime PublishDate { get; set; }

        public TimeSpan DisplayDuration { get; set; }

        public AnnouncementSeverity Severity { get; set; } = AnnouncementSeverity.Info;

        // FK vers Member (créateur)
        public int CreatedByUserId { get; set; }
        public Member? CreatedBy { get; set; }

        [NotMapped]
        public DateTime ExpirationDate => PublishDate.Add(DisplayDuration);

        [NotMapped]
        public bool IsActive => DateTime.Now >= PublishDate && DateTime.Now <= ExpirationDate;

        [NotMapped]
        public string CssClass => Severity switch
        {
            AnnouncementSeverity.Warning => "announcement-warning",
            AnnouncementSeverity.Danger => "announcement-danger",
            _ => "announcement-info"
        };

        [NotMapped]
        public string IconClass => Severity switch
        {
            AnnouncementSeverity.Warning => "fa-triangle-exclamation",
            AnnouncementSeverity.Danger => "fa-circle-xmark",
            _ => "fa-bullhorn"
        };
    }
}
