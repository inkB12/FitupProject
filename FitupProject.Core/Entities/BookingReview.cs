using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class BookingReview : BaseEntity
    {
        public string BookingId { get; set; } = string.Empty;

        public int Rating { get; set; } // 1..5
        public string? Comment { get; set; }

        public ReviewStatus Status { get; set; } = ReviewStatus.Active;

        // nav
        public Booking? Booking { get; set; }
    }
}
