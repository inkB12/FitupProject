using FitupProject.BLL.DTOs.PTs;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.BLL.Services
{
    public class PTService : IPTService
    {
        private readonly IUnitOfWork _uow;

        public PTService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PTProfileResponse?> GetProfileAsync(string accountId)
        {
            var ptRepo = _uow.GetRepository<PT>();
            var pt = await ptRepo.Entities
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.AccountId == accountId);

            if (pt == null) return null;

            return new PTProfileResponse
            {
                Id = pt.Id,
                AccountId = pt.AccountId,
                Email = pt.Account?.Email ?? string.Empty,
                Phone = pt.Account?.Phone,
                DisplayName = pt.DisplayName,
                Bio = pt.Bio,
                PricePerHour = pt.PricePerHour,
                Rating = pt.Rating,
                VerificationStatus = pt.VerificationStatus.ToString()
            };
        }
    }
}