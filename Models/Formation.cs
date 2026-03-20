using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace CreaState.Models
{
    public class Formation
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Instructor { get; set; } = "Staff Créalab";

        public DateOnly Date { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public int MaxParticipants { get; set; } = 20;
        public int CurrentParticipants { get; set; } = 0;

        // FK vers Member (créateur)
        public int CreatedByUserId { get; set; }
        public Member? CreatedBy { get; set; }

        [NotMapped]
        public TimeOnly EndTime => StartTime.Add(Duration);

        [NotMapped]
        public string FormattedTimeRange => $"{StartTime:HH\\hmm} - {EndTime:HH\\hmm}";

        [NotMapped]
        public string DayBadge => Date.Day.ToString("00");

        [NotMapped]
        public string MonthBadge => Date.ToString("MMM", CultureInfo.CreateSpecificCulture("fr-FR")).ToUpper();
    }
}
