using FitupProject.Core.Commons.Enums;

namespace FitupProject.BLL.DTOs.WorkoutPlans
{
    public class WorkoutSessionDetailDto
    {
        public string SessionId { get; set; } = default!;
        public int DayNumber { get; set; }
        public string? Notes { get; set; }
        public int Progress { get; set; }

        public List<WorkoutSessionExerciseDto> Exercises { get; set; } = new();
    }

    public class WorkoutSessionExerciseDto
    {
        public string Id { get; set; } = default!;   
        public bool IsCompleted { get; set; }
        public int Order { get; set; }
        public int? Sets { get; set; }
        public string? Reps { get; set; }
        public int? DurationSeconds { get; set; }
        public int? RestSeconds { get; set; }
        public string? Note { get; set; }

        public WorkoutMiniDto Workout { get; set; } = new();
    }

    public class WorkoutMiniDto
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Describe { get; set; }
        public string? InstructionVidLink { get; set; }

        public WorkoutLevel Level { get; set; }
        public EquipmentType Equipment { get; set; }
        public MuscleGroup PrimaryMuscle { get; set; }
        public string? Tags { get; set; }
    }
}
