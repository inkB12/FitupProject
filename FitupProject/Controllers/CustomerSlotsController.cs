using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/slots")]
    [Authorize] // Mở comment nếu bạn bắt buộc User phải đăng nhập mới được xem lịch
    public class SlotsController : ControllerBase
    {
        private readonly ISlotService _slotService;

        public SlotsController(ISlotService slotService)
        {
            _slotService = slotService;
        }

        /// <summary>
        /// API dành cho Client lấy danh sách slot rảnh của một PT cụ thể để đặt lịch
        /// </summary>
        [HttpGet("available/{ptId}")]
        public async Task<IActionResult> GetAvailableSlots(
            string ptId,
            [FromQuery] string? startDate,
            [FromQuery] string? endDate)
        {
            // 1. Validate định dạng ngày
            if (!DateOnly.TryParse(startDate, out var start))
            {
                return BadRequest(new { message = "Định dạng startDate không hợp lệ (yyyy-MM-dd)." });
            }

            // 2. Nếu không truyền endDate, mặc định lấy 1 tuần (7 ngày)
            DateOnly end;
            if (string.IsNullOrEmpty(endDate) || !DateOnly.TryParse(endDate, out end))
            {
                end = start.AddDays(7);
            }

            try
            {
                // 3. Gọi service
                var data = await _slotService.GetAvailableSlotsForClientAsync(ptId, start, end);

                return Ok(new
                {
                    success = true,
                    data = data
                });
            }
            catch (Exception ex)
            {
                // Thường là lỗi không tìm thấy PT hoặc lỗi hệ thống
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}