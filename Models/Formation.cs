using System.Globalization;

namespace CreaState.Models
{
    public class Formation
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Instructor { get; set; } = "Staff Créalab";

        public DateOnly Date { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public TimeOnly EndTime => StartTime.Add(Duration);

        public string FormattedTimeRange => $"{StartTime:HH\\hmm} - {EndTime:HH\\hmm}";

        public string DayBadge => Date.Day.ToString("00");

        public string MonthBadge => Date.ToString("MMM", CultureInfo.CreateSpecificCulture("fr-FR")).ToUpper();
    }
}

