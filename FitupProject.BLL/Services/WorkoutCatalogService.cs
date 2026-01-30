using FitupProject.BLL.DTOs.Workouts;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.BLL.Services
{
    public class WorkoutCatalogService : IWorkoutCatalogService
    {
        private readonly IUnitOfWork _uow;
        public WorkoutCatalogService(IUnitOfWork uow) => _uow = uow;

        public async Task<string> CreateWorkoutTypeAsync(string type)
        {
            type = (type ?? "").Trim();
            if (string.IsNullOrWhiteSpace(type))
                throw new Exception("WorkoutType is required.");

            var repo = _uow.GetRepository<WorkoutType>();

            var exists = await repo.Entities.AnyAsync(x => x.Type.ToLower() == type.ToLower());
            if (exists)
                throw new Exception("WorkoutType already exists.");

            var wt = new WorkoutType { Type = type };
            await repo.AddAsync(wt);
            await _uow.SaveAsync();

            return wt.Id;
        }

        public async Task<string> CreateWorkoutAsync(WorkoutCreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.WorkoutTypeId))
                throw new Exception("WorkoutTypeId is required.");

            if (string.IsNullOrWhiteSpace(req.Name))
                throw new Exception("Name is required.");

            // validate enum ints
            if (!Enum.IsDefined(typeof(WorkoutLevel), req.Level))
                throw new Exception("Invalid Level.");
            if (!Enum.IsDefined(typeof(EquipmentType), req.Equipment))
                throw new Exception("Invalid Equipment.");
            if (!Enum.IsDefined(typeof(MuscleGroup), req.PrimaryMuscle))
                throw new Exception("Invalid PrimaryMuscle.");

            // check WorkoutType exists
            var typeRepo = _uow.GetRepository<WorkoutType>();
            var typeExists = await typeRepo.Entities.AnyAsync(x => x.Id == req.WorkoutTypeId);
            if (!typeExists)
                throw new Exception("WorkoutTypeId not found.");

            var repo = _uow.GetRepository<Workout>();

            var w = new Workout
            {
                WorkoutTypeId = req.WorkoutTypeId,
                Name = req.Name.Trim(),
                Describe = req.Describe,
                InstructionVidLink = req.InstructionVidLink,
                Level = (WorkoutLevel)req.Level,
                Equipment = (EquipmentType)req.Equipment,
                PrimaryMuscle = (MuscleGroup)req.PrimaryMuscle,
                Tags = req.Tags
            };

            await repo.AddAsync(w);
            await _uow.SaveAsync();

            return w.Id;
        }

        public async Task UpdateWorkoutTypeAsync(string workoutTypeId, string newType)
        {
            newType = (newType ?? "").Trim();
            if (string.IsNullOrWhiteSpace(workoutTypeId))
                throw new Exception("WorkoutTypeId is required.");
            if (string.IsNullOrWhiteSpace(newType))
                throw new Exception("WorkoutType is required.");

            var repo = _uow.GetRepository<WorkoutType>();
            var wt = await repo.Entities.FirstOrDefaultAsync(x => x.Id == workoutTypeId);
            if (wt == null) throw new Exception("WorkoutType not found.");

            var duplicate = await repo.Entities.AnyAsync(x => x.Id != workoutTypeId && x.Type.ToLower() == newType.ToLower());
            if (duplicate) throw new Exception("WorkoutType already exists.");

            wt.Type = newType;
            _uow.GetRepository<WorkoutType>().Update(wt);
            await _uow.SaveAsync();
        }

        public async Task UpdateWorkoutAsync(string workoutId, WorkoutUpdateRequest req)
        {
            if (string.IsNullOrWhiteSpace(workoutId))
                throw new Exception("WorkoutId is required.");

            if (string.IsNullOrWhiteSpace(req.WorkoutTypeId))
                throw new Exception("WorkoutTypeId is required.");

            if (string.IsNullOrWhiteSpace(req.Name))
                throw new Exception("Name is required.");

            if (!Enum.IsDefined(typeof(WorkoutLevel), req.Level))
                throw new Exception("Invalid Level.");
            if (!Enum.IsDefined(typeof(EquipmentType), req.Equipment))
                throw new Exception("Invalid Equipment.");
            if (!Enum.IsDefined(typeof(MuscleGroup), req.PrimaryMuscle))
                throw new Exception("Invalid PrimaryMuscle.");

            // check workout type tồn tại
            var typeRepo = _uow.GetRepository<WorkoutType>();
            var typeExists = await typeRepo.Entities.AnyAsync(x => x.Id == req.WorkoutTypeId);
            if (!typeExists) throw new Exception("WorkoutTypeId not found.");

            var repo = _uow.GetRepository<Workout>();
            var w = await repo.Entities.FirstOrDefaultAsync(x => x.Id == workoutId);
            if (w == null) throw new Exception("Workout not found.");

            w.WorkoutTypeId = req.WorkoutTypeId;
            w.Name = req.Name.Trim();
            w.Describe = req.Describe;
            w.InstructionVidLink = req.InstructionVidLink;

            w.Level = (WorkoutLevel)req.Level;
            w.Equipment = (EquipmentType)req.Equipment;
            w.PrimaryMuscle = (MuscleGroup)req.PrimaryMuscle;

            w.Tags = req.Tags;

            repo.Update(w);
            await _uow.SaveAsync();
        }

        public async Task DeleteWorkoutTypeAsync(string workoutTypeId)
        {
            if (string.IsNullOrWhiteSpace(workoutTypeId))
                throw new Exception("WorkoutTypeId is required.");

            var repo = _uow.GetRepository<WorkoutType>();
            var wt = await repo.Entities.FirstOrDefaultAsync(x => x.Id == workoutTypeId);
            if (wt == null) throw new Exception("WorkoutType not found.");

            // chặn xóa nếu còn workout
            var workoutRepo = _uow.GetRepository<Workout>();
            var hasWorkouts = await workoutRepo.Entities.AnyAsync(w => w.WorkoutTypeId == workoutTypeId);
            if (hasWorkouts)
                throw new Exception("Cannot delete this WorkoutType because it still has Workouts.");

            repo.Delete(wt);
            await _uow.SaveAsync();
        }

        public async Task DeleteWorkoutAsync(string workoutId)
        {
            if (string.IsNullOrWhiteSpace(workoutId))
                throw new Exception("WorkoutId is required.");

            var repo = _uow.GetRepository<Workout>();
            var w = await repo.Entities.FirstOrDefaultAsync(x => x.Id == workoutId);
            if (w == null) throw new Exception("Workout not found.");

            // chặn xóa nếu workout đã được dùng trong plan/session
            var mapRepo = _uow.GetRepository<WorkoutSessionExercise>();
            var used = await mapRepo.Entities.AnyAsync(x => x.WorkoutId == workoutId);
            if (used)
                throw new Exception("Cannot delete this Workout because it is used in WorkoutPlans.");

            repo.Delete(w);
            await _uow.SaveAsync();
        }

        public async Task<List<WorkoutType>> GetWorkoutTypesAsync()
        {
            var repo = _uow.GetRepository<WorkoutType>();
            return await repo.Entities.OrderBy(x => x.Type).ToListAsync();
        }

        public async Task<List<Workout>> GetWorkoutsAsync(string? workoutTypeId = null)
        {
            var repo = _uow.GetRepository<Workout>();
            var q = repo.Entities.AsQueryable();

            if (!string.IsNullOrWhiteSpace(workoutTypeId))
                q = q.Where(x => x.WorkoutTypeId == workoutTypeId);

            return await q.OrderBy(x => x.Name).ToListAsync();
        }
    }
}
