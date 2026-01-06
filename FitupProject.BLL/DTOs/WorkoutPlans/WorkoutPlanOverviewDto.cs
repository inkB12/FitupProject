using FitupProject.Core.Commons.Enums;

namespace FitupProject.BLL.DTOs.WorkoutPlans
{
    public class WorkoutPlanOverviewDto
    {
        public string Id { get; set; } = default!;
        public GoalType GoalType { get; set; }
        public int Progress { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public List<WorkoutPlanWeekDto> Weeks { get; set; } = new();
    }
}
