using FitupProject.Core.Commons.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitupProject.BLL.DTOs.Booking
{
    public class GetBookingPagingRequest
    {
        public BookingStatus? Status { get; set; } 
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
