using System.Security.Claims;
using FitupProject.BLL.DTOs.Onboarding;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/onboarding")]
    [Authorize]
    public class OnboardingController : ControllerBase
    {
        private readonly IOnboardingService _svc;
        public OnboardingController(IOnboardingService svc) => _svc = svc;

        [HttpPost("submit")]
        public async Task<IActionResult> Submit(OnboardingSubmitRequest req)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var id = await _svc.SubmitAsync(accountId, req);
            return Ok(new { onboardingProfileId = id });
        }
    }
}
