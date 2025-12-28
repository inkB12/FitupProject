using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class Workout : BaseEntity
    {
        public string WorkoutTypeId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string? Describe { get; set; }
        public string? InstructionVidLink { get; set; }

        public WorkoutLevel Level { get; set; } = WorkoutLevel.Beginner;
        public EquipmentType Equipment { get; set; } = EquipmentType.Bodyweight;
        public MuscleGroup PrimaryMuscle { get; set; } = MuscleGroup.FullBody;

        // MVP: CSV/JSON string
        public string? Tags { get; set; }

        // nav
        public WorkoutType? WorkoutType { get; set; }
        public ICollection<WorkoutSessionExercise>? WorkoutSessionExercises { get; set; }
    }
}
