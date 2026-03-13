using FitupProject.BLL.DTOs.Premium;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;

namespace FitupProject.BLL.Services
{
    public class AdminPremiumService : IAdminPremiumService
    {
        private readonly IUnitOfWork _uow;

        public AdminPremiumService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PremiumTypeResponse> CreatePremiumTypeAsync(CreatePremiumTypeRequest request)
        {
            var repo = _uow.GetRepository<PremiumType>();

            var entity = new PremiumType
            {
                Describe = request.Describe,
                Duration = request.Duration,
                Price = request.Price,
                Status = PremiumTypeStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await repo.AddAsync(entity);
            await _uow.SaveAsync();

            return new PremiumTypeResponse
            {
                Id = entity.Id,
                Describe = entity.Describe,
                Duration = entity.Duration,
                Price = entity.Price,
                Status = entity.Status
            };
        }

        public async Task<PremiumTypeResponse> UpdatePremiumTypeAsync(string premiumTypeId, UpdatePremiumTypeRequest request)
        {
            var repo = _uow.GetRepository<PremiumType>();

            var entity = (await repo.FindAsync(x => x.Id == premiumTypeId)).FirstOrDefault();
            if (entity == null)
                throw new Exception("PremiumType not found.");

            entity.Describe = request.Describe;
            entity.Duration = request.Duration;
            entity.Price = request.Price;
            entity.Status = request.Status;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await repo.UpdateAsync(entity);
            await _uow.SaveAsync();

            return new PremiumTypeResponse
            {
                Id = entity.Id,
                Describe = entity.Describe,
                Duration = entity.Duration,
                Price = entity.Price,
                Status = entity.Status
            };
        }

        public async Task<bool> DeletePremiumTypeAsync(string premiumTypeId)
        {
            var repo = _uow.GetRepository<PremiumType>();

            var entity = (await repo.FindAsync(x => x.Id == premiumTypeId)).FirstOrDefault();
            if (entity == null)
                throw new Exception("PremiumType not found.");

            await repo.DeleteAsync(entity);
            await _uow.SaveAsync();

            return true;
        }

        public async Task<IEnumerable<PremiumTypeResponse>> GetAllPremiumTypesAsync()
        {
            var repo = _uow.GetRepository<PremiumType>();

            var items = await repo.FindAsync(
                predicate: null,
                orderBy: q => q.OrderBy(x => x.Duration),
                selector: x => new PremiumTypeResponse
                {
                    Id = x.Id,
                    Describe = x.Describe,
                    Duration = x.Duration,
                    Price = x.Price,
                    Status = x.Status
                });

            return items;
        }

        public async Task<IEnumerable<PremiumAdminResponse>> GetAllPremiumsAsync()
        {
            var repo = _uow.GetRepository<Premium>();

            var items = await repo.FindAsync(
                predicate: null,
                orderBy: q => q.OrderByDescending(x => x.CreatedAt),
                selector: x => new PremiumAdminResponse
                {
                    Id = x.Id,
                    AccountId = x.AccountId,
                    PremiumTypeId = x.PremiumTypeId,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = x.Status
                });

            return items;
        }

        public async Task<PremiumAdminResponse> UpdatePremiumStatusAsync(string premiumId, UpdatePremiumStatusRequest request)
        {
            var repo = _uow.GetRepository<Premium>();

            var entity = (await repo.FindAsync(x => x.Id == premiumId)).FirstOrDefault();
            if (entity == null)
                throw new Exception("Premium not found.");

            entity.Status = request.Status;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await repo.UpdateAsync(entity);
            await _uow.SaveAsync();

            return new PremiumAdminResponse
            {
                Id = entity.Id,
                AccountId = entity.AccountId,
                PremiumTypeId = entity.PremiumTypeId,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Status = entity.Status
            };
        }
    }
}
