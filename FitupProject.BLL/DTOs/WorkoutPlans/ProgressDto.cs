namespace FitupProject.BLL.DTOs.WorkoutPlans
{
    public class ProgressDto
    {
        public int TotalExercises { get; set; }
        public int CompletedExercises { get; set; }
        public int ProgressPercent { get; set; } // 0..100
    }
}
