using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class RequeteFichier
    {
        [Key]
        public int Id { get; set; }

        // FK vers Requete
        public int RequeteId { get; set; }
        public Requete? Requete { get; set; }

        [Required, MaxLength(200)]
        public string FileName { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public FileReviewStatus ReviewStatus { get; set; } = FileReviewStatus.Pending;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
