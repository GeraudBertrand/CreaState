using CreaState.DTOs.Users;

namespace CreaState.DTOs.Formations
{
    public class FormationDto
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int InstructeurId { get; set; }
        public MembreDto? Instructeur { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int MaxParticipants { get; set; }

        // Computed
        public TimeOnly EndTime { get; set; }
        public string FormattedTimeRange { get; set; } = string.Empty;
        public string DayBadge { get; set; } = string.Empty;
        public string MonthBadge { get; set; } = string.Empty;
    }
}
