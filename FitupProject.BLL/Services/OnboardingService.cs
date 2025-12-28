using FitupProject.BLL.DTOs.Onboarding;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.BLL.Services
{
    public class OnboardingService : IOnboardingService
    {
        private readonly IUnitOfWork _uow;
        public OnboardingService(IUnitOfWork uow) => _uow = uow;

        public async Task<string> SubmitAsync(string accountId, OnboardingSubmitRequest req)
        {
            var repo = _uow.GetRepository<OnboardingProfile>();

            var weeks = Math.Clamp(req.Weeks, 4, 12); //ràng buộc lại 4 đến 12 tuần cho đỡ lỗi hehe :)
            int days = Math.Clamp(req.DaysPerWeek, 3, 6);

            var profile = new OnboardingProfile
            {
                AccountId = accountId,
                GoalType = req.GoalType,
                ExperienceLevel = req.ExperienceLevel,
                Weeks = weeks,
                DaysPerWeek = days,
                MinutesPerSession = req.MinutesPerSession,
                Equipment = req.Equipment,
                FocusAreas = req.FocusAreas,
                Limitations = req.Limitations,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await repo.AddAsync(profile);
            await _uow.SaveAsync();
            return profile.Id;
        }

        public async Task<string?> GetLatestIdAsync(string accountId)
        {
            var repo = _uow.GetRepository<OnboardingProfile>();
            return await repo.Entities
                .Where(x => x.AccountId == accountId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
        }
    }
}
