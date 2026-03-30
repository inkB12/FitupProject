using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/me")]
    [Authorize]
    public class MeController : ControllerBase
    {
        private readonly IAccountService _svc;

        public MeController(IAccountService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<IActionResult> GetMe()
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
