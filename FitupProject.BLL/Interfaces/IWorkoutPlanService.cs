using FitupProject.BLL.DTOs.WorkoutPlans;

namespace FitupProject.BLL.Interfaces
{
    public interface IWorkoutPlanService
    {
        Task<string> GenerateAsync(string accountId, string onboardingProfileId);
        Task<object> GetPlanDetailAsync(string planId, string accountId);
        Task DeletePlanAsync(string planId, string accountId);

        Task<IEnumerable<WorkoutPlanSummaryDto>> GetMyPlansAsync(string accountId);
        Task<WorkoutPlanOverviewDto> GetPlanOverviewAsync(string planId, string accountId);
        Task<IEnumerable<WorkoutDayDto>> GetWeekDaysAsync(string planId, int weekNumber, string accountId);
        Task<WorkoutSessionDetailDto> GetDayDetailAsync(string planId, int weekNumber, int dayNumber, string accountId);
        Task<ProgressDto> UpdateExerciseProgressAsync(string planId, string sessionExerciseId, bool isCompleted, string accountId);
        Task<ProgressDto> GetPlanProgressAsync(string planId, string accountId);
        Task<ProgressDto> GetWeekProgressAsync(string planId, int weekNumber, string accountId);
        Task<ProgressDto> GetDayProgressAsync(string planId, int weekNumber, int dayNumber, string accountId);
    }
}
