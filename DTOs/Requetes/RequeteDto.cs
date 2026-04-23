using CreaState.DTOs.Users;

namespace CreaState.DTOs.Requetes
{
    public class RequeteDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string TypeLabel { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public string ContextLabel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string StatusCssClass { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int DemandeurId { get; set; }
        public UserDto? Demandeur { get; set; }

        public int? AssigneId { get; set; }
        public MembreDto? Assigne { get; set; }

        public List<RequeteFichierDto> Fichiers { get; set; } = [];
        public List<RequeteCommentaireDto> Commentaires { get; set; } = [];
    }
}
