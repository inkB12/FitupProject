using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitupProject.BLL.DTOs.Booking
{
    public class CreateBookingRequest
    {
        public required string SlotForBookingId { get; set; } 
        public string? Note { get; set; }          
    }
}
