using FitupProject.Core.Commons;

namespace FitupProject.Core.Entities
{
    public class WorkoutSchedule : BaseEntity
    {
        public string WorkoutPlanId { get; set; } = string.Empty;

        // quan trọng để sort/loop: 1..4
        public int WeekNumber { get; set; }

        public string? Describe { get; set; }
        public int Progress { get; set; } = 0;

        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        // nav
        public WorkoutPlan? WorkoutPlan { get; set; }
        public ICollection<WorkoutSession>? WorkoutSessions { get; set; }
    }
}
