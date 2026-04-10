using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class PrintJob
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string FileName { get; set; } = string.Empty;

        // FK vers Printer
        public int PrinterId { get; set; }
        public Printer? Printer { get; set; }

        // FK optionnelle vers Requete
        public int? RequeteId { get; set; }
        public Requete? Requete { get; set; }

        public DateTime StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public PrintStatus Status { get; set; }

        public int FilamentWeightGrams { get; set; }
    }

    public enum PrintStatus
    {
        [System.ComponentModel.DataAnnotations.Display(Name = "Terminé")]
        Success,
        [System.ComponentModel.DataAnnotations.Display(Name = "Échec")]
        Failed,
        [System.ComponentModel.DataAnnotations.Display(Name = "Annulé")]
        Cancelled
    }
}
