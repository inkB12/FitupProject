using FitupProject.BLL.DTOs.Payments;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.BLL.Services
{
    public class ConversionRateService : IConversionRateService
    {
        private readonly IUnitOfWork _uow;

        public ConversionRateService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<ConversionRate>> GetAllAsync()
        {
            return await _uow.GetRepository<ConversionRate>().Entities
                .OrderBy(x => x.Type)
                .ThenByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<ConversionRate> GetByIdAsync(string id)
        {
            var entity = await _uow.GetRepository<ConversionRate>().Entities
                .FirstOrDefaultAsync(x => x.Id == id);

            return entity ?? throw new Exception("ConversionRate not found");
        }

        public async Task<ConversionRate> CreateAsync(ConversionRateCreateDto dto)
        {
            if (dto.Rate <= 0)
                throw new Exception("Rate must be greater than 0");

            var repo = _uow.GetRepository<ConversionRate>();

            // Mỗi Type chỉ được tồn tại 1 record
            var existing = await repo.Entities
                .FirstOrDefaultAsync(x => x.Type == dto.Type);

            if (existing is not null)
            {
                // Không tạo mới, update đè record cũ
                existing.Rate = dto.Rate;
                existing.Status = dto.Status;

                repo.Update(existing);
                await _uow.SaveAsync();
                return existing;
            }

            var entity = new ConversionRate
            {
                Type = dto.Type,
                Rate = dto.Rate,
                Status = dto.Status
            };

            await repo.AddAsync(entity);
            await _uow.SaveAsync();
            return entity;
        }

        public async Task<ConversionRate> UpdateAsync(string id, ConversionRateUpdateDto dto)
        {
            if (dto.Rate <= 0)
                throw new Exception("Rate must be greater than 0");

            var repo = _uow.GetRepository<ConversionRate>();

            var entity = await repo.Entities
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity is null)
                throw new Exception("ConversionRate not found");

            // Nếu đổi Type thì phải đảm bảo không đụng record khác cùng Type
            var duplicate = await repo.Entities
                .FirstOrDefaultAsync(x => x.Type == dto.Type && x.Id != id);

            if (duplicate is not null)
                throw new Exception($"ConversionRate for type {dto.Type} already exists");

            entity.Type = dto.Type;
            entity.Rate = dto.Rate;
            entity.Status = dto.Status;

            repo.Update(entity);
            await _uow.SaveAsync();
            return entity;
        }

        public async Task DeleteAsync(string id)
        {
            var repo = _uow.GetRepository<ConversionRate>();

            var entity = await repo.Entities
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity is null)
                throw new Exception("ConversionRate not found");

            repo.Delete(entity);
            await _uow.SaveAsync();
        }
    }
}
