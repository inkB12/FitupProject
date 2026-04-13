using FitupProject.BLL.DTOs.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitupProject.BLL.Interfaces
{
    public interface IBookingService
    {
        Task<string> BookSlotAsync(CreateBookingRequest request,string userId);
        Task<IEnumerable<BookingResponse>> GetBookingsForUserAsync(string userId);
        Task CancelBookingAsync(string bookingId, string accountId);
        Task SendFeedbackAsync(SendFeedbackRequest request, string userId);
        Task<bool> ForceCancelBookingAsync(string bookingId);
        Task<IEnumerable<BookingResponse>> GetBookingsForAdminAsync(GetBookingPagingRequest request);
        Task<IEnumerable<BookingResponse>> GetBookingsForPTAsync(string ptId); Task<bool> CompleteBookingAsync(string bookingId, string ptAccountId);
    }
}
