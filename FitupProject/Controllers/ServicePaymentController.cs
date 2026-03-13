using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/service-payments")]
    [Authorize]
    public class ServicePaymentController : ControllerBase
    {
        private readonly IServicePaymentService _servicePaymentService;

        public ServicePaymentController(IServicePaymentService servicePaymentService)
        {
            _servicePaymentService = servicePaymentService;
        }

        private string? GetAccountId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("nameid");
        }

        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory()
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized("AccountId not found in token.");

            var result = await _servicePaymentService.GetMyServicePaymentHistoryAsync(accountId);
            return Ok(result);
        }

        [HttpGet("my-history/{servicePaymentId}")]
        public async Task<IActionResult> GetMyHistoryDetail(string servicePaymentId)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized("AccountId not found in token.");

            var result = await _servicePaymentService.GetMyServicePaymentDetailAsync(accountId, servicePaymentId);
            return Ok(result);
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllHistory()
        {
            var result = await _servicePaymentService.GetAllServicePaymentHistoryAsync();
            return Ok(result);
        }

        [HttpGet("admin/{servicePaymentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDetailById(string servicePaymentId)
        {
            var result = await _servicePaymentService.GetServicePaymentDetailAsync(servicePaymentId);
            return Ok(result);
        }
    }

}
