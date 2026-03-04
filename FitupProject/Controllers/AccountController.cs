using FitupProject.BLL.DTOs.Accounts;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _svc;
        public AccountController(IAccountService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _svc.GetAccountsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var acc = await _svc.GetAccountByIdAsync(id);
            if (acc == null) return NotFound(new { message = "Account not found." });
            return Ok(acc);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AccountCreateRequest req)
        {
            var id = await _svc.CreateAccountAsync(req);
            return Ok(new { id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, AccountUpdateRequest req)
        {
            await _svc.UpdateAccountAsync(id, req);
            return Ok(new { message = "Updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _svc.DeleteAccountAsync(id);
            return Ok(new { message = "Deleted successfully." });
        }
    }
}
