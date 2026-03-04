using FitupProject.BLL.DTOs.PTRegister;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/admin/pts")]
    [Authorize(Roles = "Admin")]
    public class AdminPTsController : ControllerBase
    {
        private readonly IAdminPTService _svc;

        public AdminPTsController(IAdminPTService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<IActionResult> List(
            [FromQuery] string? status,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            var data = await _svc.GetPtsAsync(status, pageIndex, pageSize);
            return Ok(new { message = "OK", data });
        }

        [HttpGet("{ptId}")]
        public async Task<IActionResult> Detail([FromRoute] string ptId)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            if (string.IsNullOrWhiteSpace(ptId))
                return BadRequest(new { message = "ptId is required." });

            var data = await _svc.GetPtDetailAsync(ptId);
            return Ok(new { message = "OK", data });
        }

        [HttpPost("{ptId}/approve")]
        public async Task<IActionResult> Approve([FromRoute] string ptId)
        {
            var reviewerId = GetAccountId();
            if (string.IsNullOrWhiteSpace(reviewerId))
                return Unauthorized(new { message = "Unauthorized." });

            if (string.IsNullOrWhiteSpace(ptId))
                return BadRequest(new { message = "ptId is required." });

            await _svc.ApproveAsync(ptId, reviewerId);
            return Ok(new { message = "PT approved.", data = (object?)null });
        }

        [HttpPost("{ptId}/reject")]
        public async Task<IActionResult> Reject([FromRoute] string ptId, [FromBody] RejectPTRequest req)
        {
            var reviewerId = GetAccountId();
            if (string.IsNullOrWhiteSpace(reviewerId))
                return Unauthorized(new { message = "Unauthorized." });

            if (string.IsNullOrWhiteSpace(ptId))
                return BadRequest(new { message = "ptId is required." });

            if (!ModelState.IsValid)
                return BadRequest(new { message = "Validation failed.", errors = ModelState });

            await _svc.RejectAsync(ptId, reviewerId, req.Reason);
            return Ok(new { message = "PT rejected.", data = (object?)null });
        }
        
        private string? GetAccountId()
        {
            return User?.Identity?.IsAuthenticated == true
                ? User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                : null;
        }
    }
}
