using FitupProject.BLL.DTOs.VNPay;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/topups")]
    [Authorize]
    public class TopUpsController : ControllerBase
    {
        private readonly ITopUpService _svc;

        public TopUpsController(ITopUpService svc) => _svc = svc;

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateTopUpDto dto)
        {
            var accountId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(accountId)) return Unauthorized();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var rs = await _svc.CreateTopUpAsync(accountId, dto, ip);
            return Ok(rs);
        }
    }
}
