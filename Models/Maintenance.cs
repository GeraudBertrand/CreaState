using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class Maintenance
    {
        [Key]
        public int Id { get; set; }

        // FK vers Printer
        public int PrinterId { get; set; }
        public Printer? Printer { get; set; }

        // FK vers Membre (worker)
        public int WorkerId { get; set; }
        public Membre? Worker { get; set; }

        public MaintenanceType Type { get; set; } = MaintenanceType.Autre;

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
