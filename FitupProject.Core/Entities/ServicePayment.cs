using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class ServicePayment : BaseEntity
    {
        public decimal Amount { get; set; } // số point bị trừ
        public ServiceType ServiceType { get; set; }

        public DateTimeOffset PaymentDate { get; set; } = DateTimeOffset.UtcNow;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // nav
        public ICollection<BookingPayment>? BookingPayments { get; set; }
        public ICollection<PremiumPayment>? PremiumPayments { get; set; }
    }
}
