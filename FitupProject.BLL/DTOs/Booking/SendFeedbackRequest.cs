using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitupProject.BLL.DTOs.Booking
{
    public class SendFeedbackRequest
    {
        public string BookingId { get; set; } = string.Empty;
        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
    }
}
