using FitupProject.BLL.DTOs.Payments;
using FitupProject.Core.Entities;

namespace FitupProject.BLL.Interfaces
{
    public interface IConversionRateService
    {
        Task<List<ConversionRate>> GetAllAsync();
        Task<ConversionRate> GetByIdAsync(string id);
        Task<ConversionRate> CreateAsync(ConversionRateCreateDto dto);
        Task<ConversionRate> UpdateAsync(string id, ConversionRateUpdateDto dto);
        Task DeleteAsync(string id);
    }
}
