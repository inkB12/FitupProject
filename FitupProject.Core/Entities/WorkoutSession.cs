using FitupProject.Core.Commons;

namespace FitupProject.Core.Entities
{
    public class WorkoutSession : BaseEntity
    {
        public string WorkoutScheduleId { get; set; } = string.Empty;

        // Day 1..7 trong tuần
        public int DayNumber { get; set; }

        public int Progress { get; set; } = 0;

        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        public string? Notes { get; set; }

        // nav
        public WorkoutSchedule? WorkoutSchedule { get; set; }
        public ICollection<WorkoutSessionExercise>? WorkoutSessionExercises { get; set; }
    }
}
