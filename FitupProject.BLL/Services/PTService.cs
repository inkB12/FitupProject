using FitupProject.BLL.DTOs.PTs;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
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

        public async Task<PTProfileResponse?> GetPTByIdAsync(string ptId)
        {
            var ptRepo = _uow.GetRepository<PT>();
            var pt = await ptRepo.Entities
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.Id == ptId && p.VerificationStatus == VerificationStatus.Verified);

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

        public async Task<IEnumerable<PTListItemResponse>> GetAllPTsAsync(PTFilterRequest? filter = null)
        {
            var ptRepo = _uow.GetRepository<PT>();
            var query = ptRepo.Entities
                .Where(p => p.VerificationStatus == VerificationStatus.Verified);

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Name))
                {
                    query = query.Where(p => p.DisplayName.ToLower().Contains(filter.Name.ToLower()));
                }

                if (filter.MinPrice.HasValue)
                {
                    query = query.Where(p => p.PricePerHour >= filter.MinPrice.Value);
                }

                if (filter.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.PricePerHour <= filter.MaxPrice.Value);
                }
            }

            return await query
                .OrderByDescending(p => p.Rating)
                .Select(p => new PTListItemResponse
                {
                    Id = p.Id,
                    DisplayName = p.DisplayName,
                    Bio = p.Bio,
                    PricePerHour = p.PricePerHour,
                    Rating = p.Rating
                })
                .ToListAsync();
        }
    }
}
