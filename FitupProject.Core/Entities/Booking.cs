using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class Booking : BaseEntity
    {
        public string SlotForBookingId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;

        public string? Note { get; set; }
        public decimal Total { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        // nav
        public SlotForBooking? SlotForBooking { get; set; }
        public Account? Account { get; set; }

        public BookingReview? BookingReview { get; set; }
        public ICollection<BookingPayment>? BookingPayments { get; set; }
    }
}
