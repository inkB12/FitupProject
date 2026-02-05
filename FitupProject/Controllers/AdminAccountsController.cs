using FitupProject.BLL.DTOs.AdminAccount;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/admin/accounts")]
    [Authorize(Roles = "Admin,Staff")]
    public class AdminAccountsController : ControllerBase
    {
        private readonly IAdminAccountService _adminAccountService;

        public AdminAccountsController(IAdminAccountService adminAccountService)
        {
            _adminAccountService = adminAccountService;
        }

        /// <summary>
        /// Get list of accounts with filter, search and pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAccounts([FromQuery] AccountListRequest request)
        {
            var result = await _adminAccountService.GetAccountsAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Get account by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(string id)
        {
            var result = await _adminAccountService.GetAccountByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Account not found." });

            return Ok(result);
        }

        /// <summary>
        /// Ban an account (set status to Suspended)
        /// </summary>
        [HttpPatch("{id}/ban")]
        public async Task<IActionResult> BanAccount(string id)
        {
            var result = await _adminAccountService.BanAccountAsync(id);
            if (result == null)
                return NotFound(new { message = "Account not found." });

            return Ok(new { message = "Account has been banned.", data = result });
        }

        /// <summary>
        /// Unban an account (set status to Active)
        /// </summary>
        [HttpPatch("{id}/unban")]
        public async Task<IActionResult> UnbanAccount(string id)
        {
            var result = await _adminAccountService.UnbanAccountAsync(id);
            if (result == null)
                return NotFound(new { message = "Account not found." });

            return Ok(new { message = "Account has been unbanned.", data = result });
        }

        /// <summary>
        /// Update account status (Inactive, PendingVerification, Active, Suspended)
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateAccountStatusRequest request)
        {
            var result = await _adminAccountService.UpdateStatusAsync(id, request.Status);
            if (result == null)
                return NotFound(new { message = "Account not found." });

            return Ok(new { message = "Account status updated.", data = result });
        }
    }
}
