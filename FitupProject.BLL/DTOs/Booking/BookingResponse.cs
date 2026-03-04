using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitupProject.BLL.DTOs.Booking
{
    public class BookingResponse
    {
        public string Id { get; set; }
        public string? SlotForBookingId { get; set; }
        public DateOnly BookingDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal Total { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
        public string? PTName { get; set; }
    }
}
