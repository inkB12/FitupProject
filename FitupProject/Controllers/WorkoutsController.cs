using FitupProject.BLL.DTOs.Workouts;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/workouts")]
    public class WorkoutsController : ControllerBase
    {
        private readonly IWorkoutCatalogService _svc;
        public WorkoutsController(IWorkoutCatalogService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? workoutTypeId)
            => Ok(await _svc.GetWorkoutsAsync(workoutTypeId));

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(WorkoutCreateRequest req)
        {
            var id = await _svc.CreateWorkoutAsync(req);
            return Ok(new { id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, WorkoutUpdateRequest req)
        {
            await _svc.UpdateWorkoutAsync(id, req);
            return Ok(new { message = "Updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _svc.DeleteWorkoutAsync(id);
            return Ok(new { message = "Deleted successfully." });
        }
    }
}
