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

            var profile = new OnboardingProfile
            {
                AccountId = accountId,
                GoalType = req.GoalType,
                ExperienceLevel = req.ExperienceLevel,
                DaysPerWeek = req.DaysPerWeek,
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
