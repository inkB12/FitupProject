using System.Security.Claims;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/pt/profile")]
    [Authorize(Roles = "PT")]
    public class PTProfileController : ControllerBase
    {
        private readonly IPTService _ptService;

        public PTProfileController(IPTService ptService)
        {
            _ptService = ptService;
        }

        private string? GetPTId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var accountId = GetPTId();
            if (string.IsNullOrEmpty(accountId)) return Unauthorized();

            var profile = await _ptService.GetProfileAsync(accountId);
            if (profile == null) return NotFound(new { message = "Không tìm thấy thông tin PT." });

            return Ok(profile);
        }
    }
}
