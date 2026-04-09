using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class Printer
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [MaxLength(20)]
        public string AccessCode { get; set; } = string.Empty;

        [MaxLength(50)]
        public string SerialNumber { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        // Navigation
        public ICollection<Maintenance> Maintenances { get; set; } = [];
        public ICollection<PrintJob> PrintJobs { get; set; } = [];
    }
}
