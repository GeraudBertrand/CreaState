using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreaState.Models
{
    public class Printer
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required, MaxLength(100)]
        public string Name { get; set; } = "Bambu Printer";

        [MaxLength(50)]
        public string Model { get; set; } = "A1 Mini";

        [Required, MaxLength(50)]
        public string IpAddress { get; set; } = "192.168.1.xxx";

        [MaxLength(20)]
        public string AccessCode { get; set; } = "000000";

        [MaxLength(50)]
        public string SerialNumber { get; set; } = "";

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
        public bool IsEnabled { get; set; } = true;

        // Navigation properties
        public ICollection<PrintJob> PrintJobs { get; set; } = [];
        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = [];

        // --- MQTT runtime state (not persisted) ---

        [NotMapped]
        public PrinterStatus Status { get; set; } = PrinterStatus.Offline;

        [NotMapped]
        public string StatusLabel => Status.GetDisplayName();

        [NotMapped]
        public string CurrentFile { get; set; } = "";

        [NotMapped]
        public int Progress { get; set; } = 0;

        [NotMapped]
        public int TimeRemainingMinutes { get; set; } = 0;

        [NotMapped]
        public int NozzleTemp { get; set; } = 0;

        [NotMapped]
        public int BedTemp { get; set; } = 0;

        [NotMapped]
        public string FilamentType { get; set; } = "PLA";

        [NotMapped]
        public string FilamentColor { get; set; } = "#ffffff";

        [NotMapped]
        public bool IsActive => Status == PrinterStatus.Printing || Status == PrinterStatus.Pause;

        public string GetStatusColor() => Status switch
        {
            PrinterStatus.Printing => "printing",
            PrinterStatus.Idle => "idle",
            PrinterStatus.Success => "success",
            PrinterStatus.Error => "error",
            _ => "offline"
        };
    }
}
