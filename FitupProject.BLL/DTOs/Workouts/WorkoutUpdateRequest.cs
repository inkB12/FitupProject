namespace FitupProject.BLL.DTOs.Workouts
{
    public class WorkoutUpdateRequest
    {
        public string WorkoutTypeId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string? Describe { get; set; }
        public string? InstructionVidLink { get; set; }

        public int Level { get; set; } = 1;
        public int Equipment { get; set; } = 1;
        public int PrimaryMuscle { get; set; } = 1;

        public string? Tags { get; set; }
    }
}
