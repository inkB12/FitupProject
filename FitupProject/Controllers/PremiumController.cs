using FitupProject.BLL.DTOs.Premium;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/premium")]
    public class PremiumController : ControllerBase
    {
        private readonly IPremiumService _premiumService;
        private readonly IAdminPremiumService _adminPremiumService;

        public PremiumController(
            IPremiumService premiumService,
            IAdminPremiumService adminPremiumService)
        {
            _premiumService = premiumService;
            _adminPremiumService = adminPremiumService;
        }

        private string? GetAccountId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("nameid");
        }

        // =========================
        // USER
        // =========================

        // POST: api/premium/purchase
        [HttpPost("purchase")]
        [Authorize]
        public async Task<IActionResult> Purchase([FromBody] PurchasePremiumRequest request)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized("AccountId not found in token.");

            var result = await _premiumService.PurchasePremiumAsync(accountId, request);
            return Ok(result);
        }

        // GET: api/premium/my-status
        [HttpGet("my-status")]
        [Authorize]
        public async Task<IActionResult> GetMyStatus()
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized("AccountId not found in token.");

            var result = await _premiumService.GetMyPremiumStatusAsync(accountId);
            return Ok(result);
        }

        // GET: api/premium/types
        [HttpGet("types")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivePremiumTypes()
        {
            var result = await _premiumService.GetActivePremiumTypesAsync();
            return Ok(result);
        }

        // =========================
        // ADMIN
        // =========================

        // POST: api/premium/admin/types
        [HttpPost("admin/types")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePremiumType([FromBody] CreatePremiumTypeRequest request)
        {
            var result = await _adminPremiumService.CreatePremiumTypeAsync(request);
            return Ok(result);
        }

        // PUT: api/premium/admin/types/{premiumTypeId}
        [HttpPut("admin/types/{premiumTypeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePremiumType(string premiumTypeId, [FromBody] UpdatePremiumTypeRequest request)
        {
            var result = await _adminPremiumService.UpdatePremiumTypeAsync(premiumTypeId, request);
            return Ok(result);
        }

        // DELETE: api/premium/admin/types/{premiumTypeId}
        [HttpDelete("admin/types/{premiumTypeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePremiumType(string premiumTypeId)
        {
            var result = await _adminPremiumService.DeletePremiumTypeAsync(premiumTypeId);
            return Ok(new { success = result });
        }

        // GET: api/premium/admin/types
        [HttpGet("admin/types")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPremiumTypes()
        {
            var result = await _adminPremiumService.GetAllPremiumTypesAsync();
            return Ok(result);
        }

        // GET: api/premium/admin/list
        [HttpGet("admin/list")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPremiums()
        {
            var result = await _adminPremiumService.GetAllPremiumsAsync();
            return Ok(result);
        }

        // PUT: api/premium/admin/{premiumId}/status
        [HttpPut("admin/{premiumId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePremiumStatus(string premiumId, [FromBody] UpdatePremiumStatusRequest request)
        {
            var result = await _adminPremiumService.UpdatePremiumStatusAsync(premiumId, request);
            return Ok(result);
        }
    }
}
