namespace FitupProject.BLL.DTOs.WorkoutPlans
{
    public class WorkoutPlanWeekDto
    {
        public string ScheduleId { get; set; } = default!;
        public int WeekNumber { get; set; }
        public string? Describe { get; set; }
        public int Progress { get; set; }
        public int TotalDays { get; set; }
    }
}
