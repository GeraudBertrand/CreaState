using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class Requete
    {
        [Key]
        public int Id { get; set; }

        public RequestType Type { get; set; } = RequestType.FDM;

        public RequestStatus Status { get; set; } = RequestStatus.Submitted;

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // FK vers User (demandeur — peut être Eleve ou Membre)
        public int DemandeurId { get; set; }
        public User? Demandeur { get; set; }

        // FK vers Membre (assigné, nullable)
        public int? AssigneId { get; set; }
        public Membre? Assigne { get; set; }

        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<RequeteFichier> Fichiers { get; set; } = [];
        public ICollection<RequeteCommentaire> Commentaires { get; set; } = [];
        public ICollection<PrintJob> PrintJobs { get; set; } = [];
    }
}
