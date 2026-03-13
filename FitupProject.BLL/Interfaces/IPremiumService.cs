using FitupProject.BLL.DTOs.Premium;

namespace FitupProject.BLL.Interfaces
{
    public interface IPremiumService
    {
        Task<PurchasePremiumResponse> PurchasePremiumAsync(string accountId, PurchasePremiumRequest request);
        Task<MyPremiumStatusResponse> GetMyPremiumStatusAsync(string accountId);
        Task<IEnumerable<PremiumTypeResponse>> GetActivePremiumTypesAsync();
    }
}
