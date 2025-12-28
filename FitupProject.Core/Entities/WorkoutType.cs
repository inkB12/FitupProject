using FitupProject.Core.Commons;

namespace FitupProject.Core.Entities
{
    public class WorkoutType : BaseEntity
    {
        public string Type { get; set; } = string.Empty;

        public ICollection<Workout>? Workouts { get; set; }
    }
}
