using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class RequeteCommentaire
    {
        [Key]
        public int Id { get; set; }

        // FK vers Requete
        public int RequeteId { get; set; }
        public Requete? Requete { get; set; }

        // FK vers User (auteur — peut être Eleve ou Membre)
        public int AuteurId { get; set; }
        public User? Auteur { get; set; }

        [Required, MaxLength(2000)]
        public string Contenu { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
