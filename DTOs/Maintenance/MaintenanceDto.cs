using CreaState.DTOs.Printers;
using CreaState.DTOs.Users;

namespace CreaState.DTOs.Maintenance
{
    public class MaintenanceDto
    {
        public int Id { get; set; }
        public int PrinterId { get; set; }
        public PrinterDto? Printer { get; set; }
        public int WorkerId { get; set; }
        public MembreDto? Worker { get; set; }
        public string Type { get; set; } = string.Empty;
        public string TypeLabel { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
