using FitupProject.Core.Commons;

namespace FitupProject.Core.Entities
{
    public class WorkoutSessionExercise : BaseEntity
    {
        public string WorkoutSessionId { get; set; } = string.Empty;
        public string WorkoutId { get; set; } = string.Empty;

        public int Order { get; set; }

        public int? Sets { get; set; }
        public string? Reps { get; set; }           // "10-12" / "AMRAP"
        public int? DurationSeconds { get; set; }   // cardio/hold
        public int? RestSeconds { get; set; }
        public string? Note { get; set; }

        // nav
        public WorkoutSession? WorkoutSession { get; set; }
        public Workout? Workout { get; set; }
    }
}
