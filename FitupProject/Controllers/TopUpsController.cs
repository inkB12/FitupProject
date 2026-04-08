using System.Security.Claims;
using FitupProject.BLL.DTOs.Payments;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/topups")]
    [Authorize]
    public class TopUpsController : ControllerBase
    {
        private readonly ITopUpService _svc;

        public TopUpsController(ITopUpService svc)
        {
            _svc = svc;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateTopUpDto dto)
        {
            var accountId = User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized();

            var result = await _svc.CreateTopUpAsync(accountId, dto);
            return Ok(result);
        }

        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetStatus(string paymentId)
        {
            var accountId = User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized();

            var result = await _svc.GetPaymentStatusAsync(paymentId, accountId);
            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyTopUps()
        {
            var accountId = User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized();

            var result = await _svc.GetMyTopUpsAsync(accountId);
            return Ok(result);
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTopUps()
        {
            var result = await _svc.GetAllTopUpsAsync();
            return Ok(result);
        }

        [HttpPost("{paymentId}/cancel-expired")]
        public async Task<IActionResult> CancelExpired(string paymentId)
        {
            var accountId = User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized();

            await _svc.CancelExpiredPendingPaymentAsync(paymentId, accountId);
            return Ok(new { message = "Payment cancelled successfully" });
        }

        [HttpPost("payos/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromBody] Webhook? webhook)
        {
            if (webhook == null)
                return BadRequest(new { message = "Webhook payload is required" });

            await _svc.HandleWebhookAsync(webhook);
            return Ok("OK");
        }

        [HttpGet("payos/return")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSReturn(
            [FromQuery] long orderCode,
            [FromQuery] string? code = null,
            [FromQuery] string? status = null,
            [FromQuery] bool? cancel = null)
        {
            await _svc.HandleReturnAsync(orderCode, code, status, cancel);

            return Ok(new
            {
                message = "Return received",
                orderCode,
                status,
                code,
                cancel
            });
        }
    }
}
