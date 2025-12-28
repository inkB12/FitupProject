using System.Security.Claims;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/workout-plans")]
    [Authorize]
    public class WorkoutPlansController : ControllerBase
    {
        private readonly IWorkoutPlanService _svc;
        private readonly IOnboardingService _onboard;

        public WorkoutPlansController(IWorkoutPlanService svc, IOnboardingService onboard)
        {
            _svc = svc;
            _onboard = onboard;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromQuery] string? onboardingProfileId)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // nếu không truyền thì lấy latest
            onboardingProfileId ??= await _onboard.GetLatestIdAsync(accountId);
            if (string.IsNullOrWhiteSpace(onboardingProfileId))
                return BadRequest(new { message = "No onboarding profile found." });

            var planId = await _svc.GenerateAsync(accountId, onboardingProfileId);
            return Ok(new { workoutPlanId = planId });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(string id)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var data = await _svc.GetPlanDetailAsync(id, accountId);
            return Ok(data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("sub")
                           ?? User.FindFirstValue("nameid");

            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            await _svc.DeletePlanAsync(id, accountId);
            return Ok(new { message = "Deleted successfully." });
        }
    }
}
