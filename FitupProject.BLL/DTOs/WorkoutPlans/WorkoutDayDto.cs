namespace FitupProject.BLL.DTOs.WorkoutPlans
{
    public class WorkoutDayDto
    {
        public string SessionId { get; set; } = default!;
        public int DayNumber { get; set; }
        public int Progress { get; set; }
        public string? Notes { get; set; }
        public int ExerciseCount { get; set; }
    }
}
