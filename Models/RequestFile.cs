using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreaState.Models
{
    public class RequestFile
    {
        [Key]
        public int Id { get; set; }

        // FK vers Request
        public int RequestId { get; set; }
        public Request? Request { get; set; }

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string OriginalFileName { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public FileReviewStatus Status { get; set; } = FileReviewStatus.Pending;

        [MaxLength(500)]
        public string? ManagerComment { get; set; }

        // Propriétés calculées
        [NotMapped]
        public string StatusLabel => Status.GetDisplayName();

        [NotMapped]
        public string FileSizeLabel
        {
            get
            {
                if (FileSize < 1024) return $"{FileSize} o";
                if (FileSize < 1024 * 1024) return $"{FileSize / 1024.0:F1} Ko";
                return $"{FileSize / 1024.0 / 1024.0:F2} Mo";
            }
        }

        [NotMapped]
        public string StatusCssClass => Status switch
        {
            FileReviewStatus.Pending => "file-pending",
            FileReviewStatus.Accepted => "file-accepted",
            FileReviewStatus.Refused => "file-refused",
            FileReviewStatus.NeedsModification => "file-modify",
            _ => ""
        };
    }
}
