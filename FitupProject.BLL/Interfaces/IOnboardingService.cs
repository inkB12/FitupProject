using FitupProject.BLL.DTOs.Onboarding;

namespace FitupProject.BLL.Interfaces
{
    public interface IOnboardingService
    {
        Task<string> SubmitAsync(string accountId, OnboardingSubmitRequest req);
        Task<string?> GetLatestIdAsync(string accountId);
    }
}
