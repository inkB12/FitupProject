using FitupProject.BLL.DTOs.Booking;
using FitupProject.BLL.Interfaces;
using FitupProject.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitupProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }


        [HttpPost("book")]
        [Authorize] 
        public async Task<IActionResult> BookSlot([FromBody] CreateBookingRequest request)
        {
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userIdFromToken))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng trong token." });
            }

           

            try
            {
                var bookingId = await _bookingService.BookSlotAsync(request,userIdFromToken);
                return Ok(new { success = true, bookingId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<IActionResult> GetMyBookings()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(accountId)) return Unauthorized();

            var data = await _bookingService.GetBookingsForUserAsync(accountId);
            return Ok(new { success = true, data = data });
        }

        [HttpDelete("my-bookings/{id}")]
        [Authorize]
        public async Task<IActionResult> CancelBooking(string id)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(accountId)) return Unauthorized();

            try
            {
                await _bookingService.CancelBookingAsync(id, accountId);

                return Ok(new
                {
                    success = true,
                    message = "Đã hủy lịch tập thành công. Khung giờ này hiện đã mở lại cho người khác."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("feedback")]
        [Authorize]
        public async Task<IActionResult> SendFeedback([FromBody] SendFeedbackRequest request)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(accountId)) return Unauthorized();

            try
            {
                await _bookingService.SendFeedbackAsync(request, accountId);
                return Ok(new { success = true, message = "Cảm ơn bạn đã gửi đánh giá cho PT!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
