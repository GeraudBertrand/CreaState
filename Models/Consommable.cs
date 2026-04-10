using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class Consommable
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Type { get; set; } = string.Empty;

        public int Quantite { get; set; }

        public int Seuil { get; set; }

        [MaxLength(50)]
        public string CouleurNom { get; set; } = string.Empty;

        [MaxLength(10)]
        public string CouleurHex { get; set; } = string.Empty;
    }
}
