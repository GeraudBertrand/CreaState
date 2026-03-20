using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreaState.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        [MaxLength(200)]
        public string Location { get; set; } = "Fablab";

        [MaxLength(50)]
        public string IconClass { get; set; } = "fa-calendar-day";

        // FK vers Member (créateur)
        public int CreatedByUserId { get; set; }
        public Member? CreatedBy { get; set; }

        [NotMapped]
        public int DaysRemaining => (Date.Date - DateTime.Now.Date).Days;

        [NotMapped]
        public bool IsUpcoming => DaysRemaining >= 0;

        [NotMapped]
        public string CountdownLabel
        {
            get
            {
                if (DaysRemaining == 0) return "Aujourd'hui !";
                if (DaysRemaining == 1) return "Demain";
                return $"J-{DaysRemaining}";
            }
        }

        [NotMapped]
        public string BadgeCssClass => DaysRemaining <= 3 ? "badge-urgent" : "badge-normal";
    }
}
