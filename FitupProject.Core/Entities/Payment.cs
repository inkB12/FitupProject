using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class Payment : BaseEntity
    {
        public string AccountId { get; set; } = string.Empty;
        public string ConversionRateId { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentMethod Method { get; set; } = PaymentMethod.PayOS;

        // payOS
        public long? OrderCode { get; set; }
        public string? CheckoutUrl { get; set; }

        public DateTimeOffset? PaidAt { get; set; }
        public DateTimeOffset? ExpiredAt { get; set; }
        public DateTimeOffset? ConfirmedAt { get; set; }

        public string? ConfirmedBy { get; set; }
        public string? ProviderTransactionId { get; set; }

        public Account? Account { get; set; }
        public ConversionRate? ConversionRate { get; set; }
    }
}
