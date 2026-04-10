namespace CreaState.DTOs.Printers
{
    public class PrinterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public bool Enabled { get; set; }
    }
}
