using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/slots")]
    [Authorize] 
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
            if (!DateOnly.TryParse(startDate, out var start))
            {
                return BadRequest(new { message = "Định dạng startDate không hợp lệ (yyyy-MM-dd)." });
            }

            
            DateOnly end;
            if (string.IsNullOrEmpty(endDate) || !DateOnly.TryParse(endDate, out end))
            {
                end = start.AddDays(7);
            }

            try
            {
                var data = await _slotService.GetAvailableSlotsForClientAsync(ptId, start, end);

                return Ok(new
                {
                    success = true,
                    data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}