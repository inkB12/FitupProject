using FitupProject.BLL.DTOs.WorkoutPlans;
using FitupProject.BLL.DTOs.Workouts;

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

        Task UpdateExerciseStatusAsync(string workoutSessionExerciseId, bool isDone, string accountId);
    }
}
