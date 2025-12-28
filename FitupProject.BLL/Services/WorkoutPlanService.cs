using FitupProject.BLL.Commons.Exceptions;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.BLL.Services
{
    public class WorkoutPlanService : IWorkoutPlanService
    {
        private readonly IUnitOfWork _uow;
        public WorkoutPlanService(IUnitOfWork uow) => _uow = uow;

        public async Task<string> GenerateAsync(string accountId, string onboardingProfileId)
        {
            var onboardRepo = _uow.GetRepository<OnboardingProfile>();
            var onboarding = await onboardRepo.Entities
                .FirstOrDefaultAsync(x => x.Id == onboardingProfileId && x.AccountId == accountId);

            if (onboarding == null) throw new ExceptionHandler("OnboardingProfile not found.");

            var focus = ParseCsv(onboarding.FocusAreas);   
            var limitations = ParseCsv(onboarding.Limitations);

            var requiredSafetyTags = new HashSet<string>();
            if (limitations.Contains("knee")) requiredSafetyTags.Add("knee-safe");
            if (limitations.Contains("back")) requiredSafetyTags.Add("back-safe");
            if (limitations.Contains("shoulder")) requiredSafetyTags.Add("shoulder-safe");
            if (limitations.Contains("no-jump")) requiredSafetyTags.Add("no-jump");

            var maxLevel = MapMaxWorkoutLevel(onboarding.ExperienceLevel);

            //candidates: filter theo equipment + level
            var workoutRepo = _uow.GetRepository<Workout>();
            var candidates = await workoutRepo.Entities
                .Where(w => w.Equipment == onboarding.Equipment)
                .Where(w => w.Level <= maxLevel)
                .ToListAsync();

            if (candidates.Count < 20)
                throw new ExceptionHandler("Not enough workouts for selected equipment/level. Add more workouts.");

            // index tags
            var tagIndex = candidates.ToDictionary(
                w => w.Id,
                w => ParseCsv(w.Tags)
            );

            // tạo plan
            var planRepo = _uow.GetRepository<WorkoutPlan>();
            var plan = new WorkoutPlan
            {
                AccountId = accountId,
                OnboardingProfileId = onboarding.Id,
                GoalType = onboarding.GoalType,
                Progress = 0,
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddDays(28),
                CreatedAt = DateTimeOffset.UtcNow
            };
            await planRepo.AddAsync(plan);
            await _uow.SaveAsync();

            var scheduleRepo = _uow.GetRepository<WorkoutSchedule>();
            var sessionRepo = _uow.GetRepository<WorkoutSession>();
            var mapRepo = _uow.GetRepository<WorkoutSessionExercise>();

            int weeks = 4;
            int days = Math.Clamp(onboarding.DaysPerWeek, 3, 6);
            var split = BuildSplit(days);

            int totalSlots = onboarding.MinutesPerSession switch
            {
                <= 15 => 4,
                <= 30 => 6,
                <= 45 => 7,
                _ => 8
            };

            //tránh trùng
            var usedInPlan = new Dictionary<string, int>();
            var rnd = new Random();

            var (baseSets, baseReps, baseRest) = BasePrescription(onboarding.GoalType, onboarding.ExperienceLevel);

            for (int week = 1; week <= weeks; week++)
            {
                var (setsDelta, repsHint, deloadFactor) = WeekProgression(week);

                var schedule = new WorkoutSchedule
                {
                    WorkoutPlanId = plan.Id,
                    WeekNumber = week,
                    Describe = $"Week {week}",
                    Progress = 0
                };
                await scheduleRepo.AddAsync(schedule);
                await _uow.SaveAsync();

                for (int d = 1; d <= days; d++)
                {
                    var dayType = split[d - 1];
                    var targets = TargetMuscles(dayType);

                    var session = new WorkoutSession
                    {
                        WorkoutScheduleId = schedule.Id,
                        DayNumber = d,
                        Progress = 0,
                        Notes = $"Focus: {dayType.ToUpperInvariant()}" + (string.IsNullOrWhiteSpace(repsHint) ? "" : $" • {repsHint}")
                    };
                    await sessionRepo.AddAsync(session);
                    await _uow.SaveAsync();

                    // ===== chọn bài theo structure =====
                    var picks = new List<Workout>();

                    // Slot 1: Warmup/mobility/cardio nhẹ
                    picks.Add(PickBest(
                        candidates,
                        w => tagIndex[w.Id].Contains("warmup") || tagIndex[w.Id].Contains("mobility"),
                        score: w =>
                        {
                            int s = 0;
                            if (tagIndex[w.Id].Contains("warmup")) s += 3;
                            if (tagIndex[w.Id].Contains("mobility")) s += 2;
                            if (requiredSafetyTags.Count > 0 && requiredSafetyTags.Except(tagIndex[w.Id]).Any()) s -= 3;
                            return s;
                        },
                        usedInPlan, week, rnd,
                        excludeIds: picks.Select(x => x.Id).ToHashSet()
                    ));

                    int mainCount = Math.Max(3, totalSlots - 2);
                    mainCount = Math.Min(mainCount, 5);

                    for (int i = 0; i < mainCount; i++)
                    {
                        picks.Add(PickBest(
                            candidates,
                            w => targets.Contains(w.PrimaryMuscle) || tagIndex[w.Id].Contains(dayType),
                            score: w =>
                            {
                                int s = 0;
                                if (targets.Contains(w.PrimaryMuscle)) s += 6;
                                if (tagIndex[w.Id].Contains(dayType)) s += 3;

                                // focus bonus
                                if (focus.Contains("core") && (w.PrimaryMuscle == MuscleGroup.Core || tagIndex[w.Id].Contains("core"))) s += 2;
                                if (focus.Contains("glutes") && (w.PrimaryMuscle == MuscleGroup.Glutes || tagIndex[w.Id].Contains("glutes"))) s += 2;

                                // limitation penalty
                                if (requiredSafetyTags.Count > 0 && requiredSafetyTags.Except(tagIndex[w.Id]).Any())
                                    s -= 8;

                                // anti-repeat: nếu dùng tuần trước => trừ
                                if (usedInPlan.TryGetValue(w.Id, out var lastWeek))
                                {
                                    var gap = week - lastWeek;
                                    if (gap <= 1) s -= 6;
                                    else if (gap == 2) s -= 2;
                                }

                                return s;
                            },
                            usedInPlan, week, rnd,
                            excludeIds: picks.Select(x => x.Id).ToHashSet()
                        ));
                    }

                    // Finisher: cardio(Lose fat) hoặc core(Gain muscle/Strength)
                    bool wantCardioFinisher = onboarding.GoalType == GoalType.LoseFat;
                    picks.Add(PickBest(
                        candidates,
                        w => wantCardioFinisher
                            ? (w.PrimaryMuscle == MuscleGroup.Cardio || tagIndex[w.Id].Contains("cardio"))
                            : (w.PrimaryMuscle == MuscleGroup.Core || tagIndex[w.Id].Contains("core")),
                        score: w =>
                        {
                            int s = 0;
                            if (wantCardioFinisher && (w.PrimaryMuscle == MuscleGroup.Cardio || tagIndex[w.Id].Contains("cardio"))) s += 6;
                            if (!wantCardioFinisher && (w.PrimaryMuscle == MuscleGroup.Core || tagIndex[w.Id].Contains("core"))) s += 6;
                            if (requiredSafetyTags.Count > 0 && requiredSafetyTags.Except(tagIndex[w.Id]).Any()) s -= 8;
                            return s;
                        },
                        usedInPlan, week, rnd,
                        excludeIds: picks.Select(x => x.Id).ToHashSet()
                    ));

                    // ===== lưu WorkoutSessionExercises =====
                    int order = 1;
                    foreach (var w in picks.Where(x => x != null))
                    {
                        usedInPlan[w.Id] = week;

                        var isCardio = w.PrimaryMuscle == MuscleGroup.Cardio || tagIndex[w.Id].Contains("cardio");
                        var isWarmup = tagIndex[w.Id].Contains("warmup") || tagIndex[w.Id].Contains("mobility");

                        int sets = isWarmup ? 1 : (int)Math.Max(1, Math.Round((baseSets + setsDelta) * deloadFactor));
                        string reps = isCardio ? "" : baseReps;
                        int rest = isWarmup ? 30 : baseRest;

                        var ex = new WorkoutSessionExercise
                        {
                            WorkoutSessionId = session.Id,
                            WorkoutId = w.Id,
                            Order = order++,

                            Sets = isCardio ? null : sets,
                            Reps = isCardio ? null : reps,
                            DurationSeconds = isCardio ? (onboarding.MinutesPerSession <= 30 ? 30 : 45) : null,
                            RestSeconds = rest,
                            Note = isWarmup ? "Warm-up" : (isCardio ? "Finisher" : null)
                        };
                        await mapRepo.AddAsync(ex);
                    }

                    await _uow.SaveAsync();
                }
            }

            return plan.Id;
        }

        // ===== Picker core =====
        private static Workout PickBest(
            List<Workout> candidates,
            Func<Workout, bool> filter,
            Func<Workout, int> score,
            Dictionary<string, int> usedInPlan,
            int week,
            Random rnd,
            HashSet<string> excludeIds)
        {
            var list = candidates
                .Where(filter)
                .Where(w => !excludeIds.Contains(w.Id))
                .Select(w => new { w, s = score(w) })
                .OrderByDescending(x => x.s)
                .Take(12)
                .ToList();

            if (list.Count == 0)
            {
                //lấy ngẫu nhiên bài chưa dùng
                var fb = candidates.FirstOrDefault(w => !excludeIds.Contains(w.Id));
                return fb!;
            }

            // random nhẹ trong để plan không giống nhau hehe :))
            var pick = list[rnd.Next(Math.Min(4, list.Count))].w;
            return pick;
        }


        public async Task<object> GetPlanDetailAsync(string planId, string accountId)
        {
            var planRepo = _uow.GetRepository<WorkoutPlan>();

            // load full tree
            var plan = await planRepo.Entities
                .Where(p => p.Id == planId && p.AccountId == accountId)
                .Include(p => p.WorkoutSchedules!)
                    .ThenInclude(w => w.WorkoutSessions!)
                        .ThenInclude(s => s.WorkoutSessionExercises!)
                            .ThenInclude(e => e.Workout)
                .FirstOrDefaultAsync();

            if (plan == null) throw new ExceptionHandler("WorkoutPlan not found.");

            // return DTO-like anonymous object
            return new
            {
                plan.Id,
                plan.GoalType,
                plan.StartDate,
                plan.EndDate,
                weeks = plan.WorkoutSchedules!
                    .OrderBy(x => x.WeekNumber)
                    .Select(w => new
                    {
                        w.WeekNumber,
                        w.Describe,
                        days = w.WorkoutSessions!
                            .OrderBy(s => s.DayNumber)
                            .Select(s => new
                            {
                                s.DayNumber,
                                s.Progress,
                                exercises = s.WorkoutSessionExercises!
                                    .OrderBy(e => e.Order)
                                    .Select(e => new
                                    {
                                        e.Order,
                                        e.Sets,
                                        e.Reps,
                                        e.DurationSeconds,
                                        e.RestSeconds,
                                        workout = new
                                        {
                                            e.Workout!.Id,
                                            e.Workout.Name,
                                            e.Workout.Describe,
                                            e.Workout.InstructionVidLink,
                                            e.Workout.Level,
                                            e.Workout.Equipment,
                                            e.Workout.PrimaryMuscle,
                                            e.Workout.Tags
                                        }
                                    })
                            })
                    })
            };
        }

        public async Task DeletePlanAsync(string planId, string accountId)
        {
            if (string.IsNullOrWhiteSpace(planId))
                throw new ExceptionHandler("PlanId is required.");

            // check plan thuộc account
            var planRepo = _uow.GetRepository<WorkoutPlan>();
            var plan = await planRepo.Entities
                .FirstOrDefaultAsync(p => p.Id == planId && p.AccountId == accountId);

            if (plan == null)
                throw new ExceptionHandler("WorkoutPlan not found.");

            // lấy schedules của plan
            var scheduleRepo = _uow.GetRepository<WorkoutSchedule>();
            var schedules = await scheduleRepo.Entities
                .Where(s => s.WorkoutPlanId == planId)
                .Select(s => s.Id)
                .ToListAsync();

            if (schedules.Count > 0)
            {
                // lấy sessions
                var sessionRepo = _uow.GetRepository<WorkoutSession>();
                var sessions = await sessionRepo.Entities
                    .Where(ss => schedules.Contains(ss.WorkoutScheduleId))
                    .Select(ss => ss.Id)
                    .ToListAsync();

                if (sessions.Count > 0)
                {
                    // xoá exercises trước
                    var exRepo = _uow.GetRepository<WorkoutSessionExercise>();
                    var exercises = await exRepo.Entities
                        .Where(e => sessions.Contains(e.WorkoutSessionId))
                        .ToListAsync();

                    if (exercises.Count > 0)
                        exRepo.DeleteRange(exercises);

                    // xoá sessions
                    var sessionEntities = await sessionRepo.Entities
                        .Where(ss => sessions.Contains(ss.Id))
                        .ToListAsync();

                    if (sessionEntities.Count > 0)
                        sessionRepo.DeleteRange(sessionEntities);
                }

                // xoá schedules
                var scheduleEntities = await scheduleRepo.Entities
                    .Where(s => s.WorkoutPlanId == planId)
                    .ToListAsync();

                if (scheduleEntities.Count > 0)
                    scheduleRepo.DeleteRange(scheduleEntities);
            }

            // xoá plan
            planRepo.Delete(plan);
            await _uow.SaveAsync();
        }

        //Helpers
        private static HashSet<string> ParseCsv(string? csv)
        {
            return (csv ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.Trim().ToLowerInvariant())
                .ToHashSet();
        }

        private static WorkoutLevel MapMaxWorkoutLevel(ExperienceLevel exp)
        {
            return exp switch
            {
                ExperienceLevel.Beginner => WorkoutLevel.Beginner,
                ExperienceLevel.Intermediate => WorkoutLevel.Intermediate,
                _ => WorkoutLevel.Advanced
            };
        }

        private static List<string> BuildSplit(int days)
        {
            days = Math.Clamp(days, 3, 6);

            return days switch
            {
                3 => new() { "fullbody", "fullbody", "fullbody" },
                4 => new() { "upper", "lower", "upper", "lower" },
                5 => new() { "upper", "lower", "push", "pull", "legs" },
                _ => new() { "push", "pull", "legs", "push", "pull", "legs" }
            };
        }

        private static HashSet<MuscleGroup> TargetMuscles(string dayType)
        {
            return dayType switch
            {
                "upper" => new() { MuscleGroup.Chest, MuscleGroup.Back, MuscleGroup.Shoulders, MuscleGroup.Arms, MuscleGroup.Core },
                "lower" => new() { MuscleGroup.Legs, MuscleGroup.Glutes, MuscleGroup.Core },
                "push" => new() { MuscleGroup.Chest, MuscleGroup.Shoulders, MuscleGroup.Arms, MuscleGroup.Core },
                "pull" => new() { MuscleGroup.Back, MuscleGroup.Arms, MuscleGroup.Core },
                "legs" => new() { MuscleGroup.Legs, MuscleGroup.Glutes, MuscleGroup.Core },
                _ => new() { MuscleGroup.FullBody, MuscleGroup.Core, MuscleGroup.Cardio, MuscleGroup.Legs, MuscleGroup.Back, MuscleGroup.Chest }
            };
        }

        private static (int sets, string reps, int rest) BasePrescription(GoalType goal, ExperienceLevel exp)
        {
            return goal switch
            {
                GoalType.LoseFat => (3, exp == ExperienceLevel.Beginner ? "12-15" : "10-15", 45),
                GoalType.GainMuscle => (3, exp == ExperienceLevel.Beginner ? "10-12" : "8-12", 60),
                GoalType.Strength => (4, exp == ExperienceLevel.Beginner ? "6-8" : "4-6", 120),
                _ => (3, "10-12", 60)
            };
        }

        private static (int setsDelta, string repsHint, double deloadFactor) WeekProgression(int week)
        {
            // week: 1..4
            return week switch
            {
                1 => (0, "", 1.0),
                2 => (+1, "", 1.0),
                3 => (+1, "+2 reps if easy", 1.0),
                4 => (-1, "deload", 0.75), // giảm volume
                _ => (0, "", 1.0)
            };
        }

    }
}
