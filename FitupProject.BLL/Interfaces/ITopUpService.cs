using FitupProject.BLL.DTOs.Payments;
using PayOS.Models.Webhooks;

namespace FitupProject.BLL.Interfaces
{
    public interface ITopUpService
    {
        Task<CreateTopUpResultDto> CreateTopUpAsync(string accountId, CreateTopUpDto dto);
        Task<IEnumerable<PaymentListItemDto>> GetMyTopUpsAsync(string accountId);
        Task<IEnumerable<PaymentListItemDto>> GetAllTopUpsAsync();
        Task<PaymentStatusDto> GetPaymentStatusAsync(string paymentId, string accountId);
        Task CancelExpiredPendingPaymentAsync(string paymentId, string accountId);
        Task HandleWebhookAsync(Webhook? webhook);

        Task HandleReturnAsync(long orderCode, string? code, string? status, bool? cancel);
    }
}
