using System.Security.Claims;
using FitupProject.BLL.DTOs.WorkoutPlans;
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

        //lấy token
        private string? GetAccountId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("nameid");
        }

        //generate
        // POST /api/workout-plans/generate?onboardingProfileId=...
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromQuery] string? onboardingProfileId)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            // nếu không truyền thì lấy latest
            onboardingProfileId ??= await _onboard.GetLatestIdAsync(accountId);
            if (string.IsNullOrWhiteSpace(onboardingProfileId))
                return BadRequest(new { message = "No onboarding profile found." });

            var planId = await _svc.GenerateAsync(accountId, onboardingProfileId);
            return Ok(new { workoutPlanId = planId });
        }

        //my plans (summary list)
        // GET /api/workout-plans
        [HttpGet]
        public async Task<IActionResult> GetMyPlans()
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            var data = await _svc.GetMyPlansAsync(accountId);
            return Ok(data);
        }

        //overview 1 plan (weeks summary)
        // GET /api/workout-plans/{id}/overview
        [HttpGet("{id}/overview")]
        public async Task<IActionResult> GetOverview(string id)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            var data = await _svc.GetPlanOverviewAsync(id, accountId);
            return Ok(data);
        }

        //days in a week
        // GET /api/workout-plans/{id}/weeks/{weekNumber}
        [HttpGet("{id}/weeks/{weekNumber:int}")]
        public async Task<IActionResult> GetWeekDays(string id, int weekNumber)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            var data = await _svc.GetWeekDaysAsync(id, weekNumber, accountId);
            return Ok(data);
        }

        //day detail (exercises)
        // GET /api/workout-plans/{id}/weeks/{weekNumber}/days/{dayNumber}
        [HttpGet("{id}/weeks/{weekNumber:int}/days/{dayNumber:int}")]
        public async Task<IActionResult> GetDayDetail(string id, int weekNumber, int dayNumber)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            var data = await _svc.GetDayDetailAsync(id, weekNumber, dayNumber, accountId);
            return Ok(data);
        }

        //full detail tree
        // GET /api/workout-plans/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(string id)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            var data = await _svc.GetPlanDetailAsync(id, accountId);
            return Ok(data);
        }

        //delete plan
        // DELETE /api/workout-plans/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            await _svc.DeletePlanAsync(id, accountId);
            return Ok(new { message = "Deleted successfully." });
        }

        //update status WorkoutSessionExercise
        [HttpPatch("{planId}/exercises/{sessionExerciseId}")]
        public async Task<IActionResult> UpdateExerciseProgress(string planId, string sessionExerciseId, [FromBody] UpdateExerciseProgressRequest req)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("sub")
                           ?? User.FindFirstValue("nameid");

            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            var progress = await _svc.UpdateExerciseProgressAsync(planId, sessionExerciseId, req.IsCompleted, accountId);
            return Ok(progress);
        }

        //progress plan
        [HttpGet("{planId}/progress")]
        public async Task<IActionResult> GetPlanProgress(string planId)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("sub")
                           ?? User.FindFirstValue("nameid");

            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            var progress = await _svc.GetPlanProgressAsync(planId, accountId);
            return Ok(progress);
        }

        //progress week
        [HttpGet("{planId}/weeks/{weekNumber}/progress")]
        public async Task<IActionResult> GetWeekProgress(string planId, int weekNumber)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("sub")
                           ?? User.FindFirstValue("nameid");

            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            var progress = await _svc.GetWeekProgressAsync(planId, weekNumber, accountId);
            return Ok(progress);
        }

        //progress day
        [HttpGet("{planId}/weeks/{weekNumber}/days/{dayNumber}/progress")]
        public async Task<IActionResult> GetDayProgress(string planId, int weekNumber, int dayNumber)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("sub")
                           ?? User.FindFirstValue("nameid");

            if (string.IsNullOrWhiteSpace(accountId))
                return Unauthorized(new { message = "Unauthorized." });

            var progress = await _svc.GetDayProgressAsync(planId, weekNumber, dayNumber, accountId);
            return Ok(progress);
        }
    }
}
