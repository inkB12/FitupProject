using FitupProject.BLL.DTOs.Workouts;
using FitupProject.Core.Entities;

namespace FitupProject.BLL.Interfaces
{
    public interface IWorkoutCatalogService
    {
        Task<string> CreateWorkoutTypeAsync(string type);
        Task<string> CreateWorkoutAsync(WorkoutCreateRequest req);

        Task UpdateWorkoutTypeAsync(string workoutTypeId, string newType);
        Task DeleteWorkoutTypeAsync(string workoutTypeId);

        Task UpdateWorkoutAsync(string workoutId, WorkoutUpdateRequest req);
        Task DeleteWorkoutAsync(string workoutId);

        Task<List<WorkoutType>> GetWorkoutTypesAsync();
        Task<List<Workout>> GetWorkoutsAsync(string? workoutTypeId = null);
    }
}
