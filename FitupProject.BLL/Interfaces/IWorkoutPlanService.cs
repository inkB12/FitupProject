namespace FitupProject.BLL.Interfaces
{
    public interface IWorkoutPlanService
    {
        Task<string> GenerateAsync(string accountId, string onboardingProfileId);
        Task<object> GetPlanDetailAsync(string planId, string accountId);

        Task DeletePlanAsync(string planId, string accountId);
    }
}
