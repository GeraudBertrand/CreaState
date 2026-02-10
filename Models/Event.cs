namespace CreaState.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Location { get; set; } = "Fablab";

        public string IconClass { get; set; } = "fa-calendar-day";

        public int DaysRemaining => (Date.Date - DateTime.Now.Date).Days;

        public bool IsUpcoming => DaysRemaining >= 0;

        public string CountdownLabel
        {
            get
            {
                if (DaysRemaining == 0) return "Aujourd'hui !";
                if (DaysRemaining == 1) return "Demain";
                return $"J-{DaysRemaining}";
            }
        }

        public string BadgeCssClass => DaysRemaining <= 3 ? "badge-urgent" : "badge-normal";
    }
}
