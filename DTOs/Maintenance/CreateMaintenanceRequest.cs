using CreaState.Models;

namespace CreaState.DTOs.Maintenance
{
    public class CreateMaintenanceRequest
    {
        public int PrinterId { get; set; }
        public int WorkerId { get; set; }
        public MaintenanceType Type { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
