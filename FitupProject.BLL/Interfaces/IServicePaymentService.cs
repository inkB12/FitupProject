using FitupProject.BLL.DTOs.ServicePayment;

namespace FitupProject.BLL.Interfaces
{
    public interface IServicePaymentService
    {
        Task<IEnumerable<ServicePaymentHistoryResponse>> GetMyServicePaymentHistoryAsync(string accountId);
        Task<IEnumerable<ServicePaymentHistoryResponse>> GetAllServicePaymentHistoryAsync();

        Task<ServicePaymentDetailResponse> GetServicePaymentDetailAsync(string servicePaymentId);
        Task<ServicePaymentDetailResponse> GetMyServicePaymentDetailAsync(string accountId, string servicePaymentId);
    }
}
