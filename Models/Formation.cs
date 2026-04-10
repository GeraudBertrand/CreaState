using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class Formation
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Titre { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // FK vers Membre (instructeur)
        public int InstructeurId { get; set; }
        public Membre? Instructeur { get; set; }

        public DateOnly Date { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public int MaxParticipants { get; set; } = 20;
    }
}
