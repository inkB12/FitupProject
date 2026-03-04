using FitupProject.BLL.DTOs.PTRegister;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/pt")]
    public class PTController : ControllerBase
    {
        private readonly IPTService _svc;

        public PTController(IPTService svc)
        {
            _svc = svc;
        }

        [HttpPost("register")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Register([FromBody] PTRegisterRequest req)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            // ModelState validation (DataAnnotations)
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Validation failed.", errors = ModelState });

            var data = await _svc.RegisterAsync(accountId, req);
            return Ok(new { message = "PT registration submitted.", data });
        }

        [HttpGet("me")]
        [Authorize(Roles = "User,PT")]
        public async Task<IActionResult> Me()
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            var data = await _svc.GetMeAsync(accountId);
            return Ok(new { message = "OK", data });
        }

        private string? GetAccountId()
        {
            return User?.Identity?.IsAuthenticated == true
                ? User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                : null;
        }
    }
}
