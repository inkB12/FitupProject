using FitupProject.BLL.DTOs.Premium;

namespace FitupProject.BLL.Interfaces
{
    public interface IAdminPremiumService
    {
        Task<PremiumTypeResponse> CreatePremiumTypeAsync(CreatePremiumTypeRequest request);
        Task<PremiumTypeResponse> UpdatePremiumTypeAsync(string premiumTypeId, UpdatePremiumTypeRequest request);
        Task<bool> DeletePremiumTypeAsync(string premiumTypeId);
        Task<IEnumerable<PremiumTypeResponse>> GetAllPremiumTypesAsync();

        Task<IEnumerable<PremiumAdminResponse>> GetAllPremiumsAsync();
        Task<PremiumAdminResponse> UpdatePremiumStatusAsync(string premiumId, UpdatePremiumStatusRequest request);
    }
}
