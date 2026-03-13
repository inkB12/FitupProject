using FitupProject.Core.Commons.Enums;

namespace FitupProject.BLL.DTOs.ServicePayment
{
    public class ServicePaymentHistoryResponse
    {
        public string ServicePaymentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public ServiceType ServiceType { get; set; }
        public DateTimeOffset PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }

        public string? PremiumId { get; set; }
        public string? BookingId { get; set; }

        public string? AccountId { get; set; }
    }
}
