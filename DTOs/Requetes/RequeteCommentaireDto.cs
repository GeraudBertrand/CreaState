using CreaState.DTOs.Users;

namespace CreaState.DTOs.Requetes
{
    public class RequeteCommentaireDto
    {
        public int Id { get; set; }
        public int RequeteId { get; set; }
        public int AuteurId { get; set; }
        public UserDto? Auteur { get; set; }
        public string Contenu { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
