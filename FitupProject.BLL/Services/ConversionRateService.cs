using FitupProject.BLL.DTOs.VNPay;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.BLL.Services
{
    public class ConversionRateService : IConversionRateService
    {
        private readonly IUnitOfWork _uow;
        public ConversionRateService(IUnitOfWork uow) => _uow = uow;

        public async Task<List<ConversionRate>> GetAllAsync()
            => await _uow.GetRepository<ConversionRate>().Entities
                .OrderByDescending(x => x.CreatedAt) // nếu BaseEntity có CreatedAt
                .ToListAsync();

        public async Task<ConversionRate> GetByIdAsync(string id)
        {
            var e = await _uow.GetRepository<ConversionRate>().Entities.FirstOrDefaultAsync(x => x.Id == id);
            return e ?? throw new Exception("ConversionRate not found");
        }

        public async Task<ConversionRate> CreateAsync(ConversionRateCreateDto dto)
        {
            if (dto.Rate <= 0) throw new Exception("Rate must be > 0");

            var repo = _uow.GetRepository<ConversionRate>();
            var e = new ConversionRate { Rate = dto.Rate, Status = dto.Status };
            await repo.AddAsync(e);
            await _uow.SaveAsync();
            return e;
        }

        public async Task<ConversionRate> UpdateAsync(string id, ConversionRateUpdateDto dto)
        {
            var repo = _uow.GetRepository<ConversionRate>();
            var e = await repo.Entities.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new Exception("ConversionRate not found");

            e.Rate = dto.Rate;
            e.Status = dto.Status;

            repo.Update(e);
            await _uow.SaveAsync();
            return e;
        }

        public async Task DeleteAsync(string id)
        {
            var repo = _uow.GetRepository<ConversionRate>();
            var e = await repo.Entities.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new Exception("ConversionRate not found");

            repo.Delete(e);
            await _uow.SaveAsync();
        }
    }
}
