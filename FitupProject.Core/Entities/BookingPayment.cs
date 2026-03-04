using FitupProject.Core.Commons;

namespace FitupProject.Core.Entities
{
    public class BookingPayment : BaseEntity
    {
        public string BookingId { get; set; } = string.Empty;
        public string ServicePaymentId { get; set; } = string.Empty;

        public decimal Price { get; set; } // giá của booking (point)

        // nav
        public Booking? Booking { get; set; }
        public ServicePayment? ServicePayment { get; set; }
    }
}
