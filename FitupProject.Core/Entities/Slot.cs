using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class Slot : BaseEntity
    {
        public string PTId { get; set; } = string.Empty;

        public TimeOnly SlotStart { get; set; }
        public TimeOnly SlotEnd { get; set; }

        // Ngày trong tuần (0=Sunday..6=Saturday) hoặc bạn map DayOfWeek
        public DayOfWeek DateInWeek { get; set; }

        public decimal Price { get; set; }
        public SlotStatus Status { get; set; } = SlotStatus.Active;

        // nav
        public PT? PT { get; set; }
        public ICollection<SlotForBooking>? SlotForBookings { get; set; }
    }
}
