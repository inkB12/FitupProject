using FitupProject.Core.Commons.Enums;

namespace FitupProject.BLL.DTOs.ServicePayment
{
    public class ServicePaymentDetailResponse
    {
        public string ServicePaymentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public ServiceType ServiceType { get; set; }
        public DateTimeOffset PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }

        public PremiumPaymentDetailDto? PremiumPaymentDetail { get; set; }
        public BookingPaymentDetailDto? BookingPaymentDetail { get; set; }
    }

    public class PremiumPaymentDetailDto
    {
        public string PremiumPaymentId { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public string PremiumId { get; set; } = string.Empty;
        public string PremiumTypeId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;

        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public PremiumStatus PremiumStatus { get; set; }
    }

    public class BookingPaymentDetailDto
    {
        public string BookingPaymentId { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public string BookingId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string SlotForBookingId { get; set; } = string.Empty;

        public decimal Total { get; set; }
        public string? Note { get; set; }
        public BookingStatus BookingStatus { get; set; }
    }
}
