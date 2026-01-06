using FitupProject.Core.Commons.Enums;

namespace FitupProject.BLL.DTOs.WorkoutPlans
{
    public class WorkoutPlanSummaryDto
    {
        public string Id { get; set; } = default!;
        public GoalType GoalType { get; set; }
        public int Progress { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public int TotalWeeks { get; set; }
        public int TotalSessions { get; set; }
    }
}
