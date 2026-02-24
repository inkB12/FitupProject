using System.Security.Claims;
using FitupProject.BLL.DTOs.Slots;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/pt/slots")]
    [Authorize(Roles = "PT")]
    public class PTSlotsController : ControllerBase
    {
        private readonly ISlotService _slotService;

        public PTSlotsController(ISlotService slotService)
        {
            _slotService = slotService;
        }

        private string? GetPTId()
        {
            // Giả sử bạn dùng AccountId để định danh PT trong hệ thống
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
        }

        [HttpPost]
        public async Task<IActionResult> CreateSlot([FromBody] CreateSlotRequest request)
        {
            var ptId = GetPTId();
            if (string.IsNullOrEmpty(ptId)) return Unauthorized();

            // Gán PTId từ token để bảo mật
            request.PTId = ptId;

            try
            {
                var id = await _slotService.CreateSlotAsync(request);
                return Ok(new { slotId = id, message = "Đăng ký lịch rảnh thành công và đã sinh lịch cho 4 tuần tới." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMySlots()
        {
            var ptId = GetPTId();
            if (string.IsNullOrEmpty(ptId)) return Unauthorized();

            var data = await _slotService.GetSlotsAsync(ptId);
            return Ok(data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSlot(string id)
        {
            var ptId = GetPTId();
            if (string.IsNullOrEmpty(ptId)) return Unauthorized();

            try
            {
                await _slotService.DeleteSlotAsync(id, ptId);
                return Ok(new { message = "Đã xóa lịch mẫu và hủy các lịch rảnh tương ứng trong tương lai." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> GetCalendar([FromQuery] string startDate)
        {
            var ptId = GetPTId();
            if (string.IsNullOrEmpty(ptId)) return Unauthorized();

            if (!DateOnly.TryParse(startDate, out var date))
            {
                return BadRequest(new { message = "Định dạng ngày không hợp lệ (yyyy-MM-dd)." });
            }

            var data = await _slotService.GetWeeklyCalendarAsync(ptId, date);
            return Ok(data);
        }

        [HttpDelete("calendar/{id}")]
        public async Task<IActionResult> CancelSpecificSlot(string id)
        {
            var ptId = GetPTId();
            if (string.IsNullOrEmpty(ptId)) return Unauthorized();

            try
            {
                await _slotService.CancelSlotForBookingAsync(id, ptId);
                return Ok(new { message = "Đã hủy lịch rảnh thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
