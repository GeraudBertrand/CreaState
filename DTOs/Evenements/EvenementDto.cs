namespace CreaState.DTOs.Evenements
{
    public class EvenementDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Icone { get; set; } = string.Empty;

        // Computed
        public int DaysRemaining { get; set; }
        public bool IsUpcoming { get; set; }
        public string CountdownLabel { get; set; } = string.Empty;
        public string BadgeCssClass { get; set; } = string.Empty;
    }
}
