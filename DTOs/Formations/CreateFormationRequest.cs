using System.ComponentModel.DataAnnotations;

namespace CreaState.DTOs.Formations
{
    public class CreateFormationRequest
    {
        [Required, MaxLength(200)]
        public string Titre { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public int InstructeurId { get; set; }

        public DateOnly Date { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public int MaxParticipants { get; set; } = 20;
    }
}
