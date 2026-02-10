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
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime PublishDate { get; set; }

        public TimeSpan DisplayDuration { get; set; }

        public AnnouncementSeverity Severity { get; set; } = AnnouncementSeverity.Info;

        public DateTime ExpirationDate => PublishDate.Add(DisplayDuration);

        public bool IsActive => DateTime.Now >= PublishDate && DateTime.Now <= ExpirationDate;

        public string CssClass => Severity switch
        {
            AnnouncementSeverity.Warning => "announcement-warning",
            AnnouncementSeverity.Danger => "announcement-danger",
            _ => "announcement-info"
        };

        public string IconClass => Severity switch
        {
            AnnouncementSeverity.Warning => "fa-triangle-exclamation",
            AnnouncementSeverity.Danger => "fa-circle-xmark",
            _ => "fa-bullhorn"
        };
    }
}
