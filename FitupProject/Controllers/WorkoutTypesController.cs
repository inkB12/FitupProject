using FitupProject.BLL.Interfaces;
using FitupProject.Models.Workouts;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/workout-types")]
    public class WorkoutTypesController : ControllerBase
    {
        private readonly IWorkoutCatalogService _svc;
        public WorkoutTypesController(IWorkoutCatalogService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _svc.GetWorkoutTypesAsync());

        // [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(WorkoutTypeCreateRequest req)
        {
            var id = await _svc.CreateWorkoutTypeAsync(req.Type);
            return Ok(new { id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, WorkoutTypeUpdateRequest req)
        {
            await _svc.UpdateWorkoutTypeAsync(id, req.Type);
            return Ok(new { message = "Updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _svc.DeleteWorkoutTypeAsync(id);
            return Ok(new { message = "Deleted successfully." });
        }
    }
}
