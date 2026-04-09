using System.ComponentModel.DataAnnotations;

namespace CreaState.DTOs.Evenements
{
    public class CreateEvenementRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Icone { get; set; } = string.Empty;
    }
}
