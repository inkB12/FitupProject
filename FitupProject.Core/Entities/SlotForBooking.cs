using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class SlotForBooking : BaseEntity
    {
        public string SlotId { get; set; } = string.Empty;

        public DateOnly BookingDate { get; set; } // ngày cụ thể
        public decimal Price { get; set; }

        public SlotForBookingStatus Status { get; set; } = SlotForBookingStatus.Available;

        // nav
        public Slot? Slot { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
    }
}
