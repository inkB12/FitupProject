using System;
using System.Text.Json.Serialization;

namespace FitupProject.BLL.DTOs.Slots
{
    public class CreateSlotRequest
    {
        [JsonIgnore]
        public string PTId { get; set; } = string.Empty;
        public TimeOnly SlotStart { get; set; }
        public TimeOnly SlotEnd { get; set; }
        public DayOfWeek DateInWeek { get; set; }
        public decimal Price { get; set; }
    }

    public class SlotResponse
    {
        public string Id { get; set; } = string.Empty;
        public string PTId { get; set; } = string.Empty;
        public TimeOnly SlotStart { get; set; }
        public TimeOnly SlotEnd { get; set; }
        public DayOfWeek DateInWeek { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SlotForBookingResponse
    {
        public string Id { get; set; } = string.Empty;
        public DateOnly BookingDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
