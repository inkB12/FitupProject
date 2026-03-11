using FitupProject.BLL.DTOs.WorkoutPlans;
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

            if (onboarding == null) throw new Exception("OnboardingProfile not found.");

            int weeks = Math.Clamp(onboarding.Weeks, 4, 12);
            int days = Math.Clamp(onboarding.DaysPerWeek, 3, 6);
            var split = BuildSplit(days);

            var focus = ParseCsv(onboarding.FocusAreas);
            var limitations = ParseCsv(onboarding.Limitations);

            var requiredSafetyTags = new HashSet<string>();
            if (limitations.Contains("knee")) requiredSafetyTags.Add("knee-safe");
            if (limitations.Contains("back")) requiredSafetyTags.Add("back-safe");
            if (limitations.Contains("shoulder")) requiredSafetyTags.Add("shoulder-safe");
            if (limitations.Contains("no-jump")) requiredSafetyTags.Add("no-jump");

            var maxLevel = MapMaxWorkoutLevel(onboarding.ExperienceLevel);

            var workoutRepo = _uow.GetRepository<Workout>();
            var candidates = await workoutRepo.Entities
                .Where(w => w.Equipment == onboarding.Equipment)
                .Where(w => w.Level <= maxLevel)
                .ToListAsync();

            if (candidates.Count < 20)
                throw new Exception("Not enough workouts for selected equipment/level. Add more workouts.");

            // index tags
            var tagIndex = candidates.ToDictionary(
                w => w.Id,
                w => ParseCsv(w.Tags)
            );

            // tạo plan
            var start = DateTimeOffset.UtcNow;

            var planRepo = _uow.GetRepository<WorkoutPlan>();
            var plan = new WorkoutPlan
            {
                AccountId = accountId,
                OnboardingProfileId = onboarding.Id,
                GoalType = onboarding.GoalType,
                Progress = 0,
                StartDate = start,
                EndDate = start.AddDays(weeks * 7), 
                CreatedAt = start
            };

            await planRepo.AddAsync(plan);
            await _uow.SaveAsync();

            var scheduleRepo = _uow.GetRepository<WorkoutSchedule>();
            var sessionRepo = _uow.GetRepository<WorkoutSession>();
            var mapRepo = _uow.GetRepository<WorkoutSessionExercise>();

            int totalSlots = onboarding.MinutesPerSession switch
            {
                <= 15 => 4,
                <= 30 => 6,
                <= 45 => 7,
                _ => 8
            };

            // tránh trùng
            var usedInPlan = new Dictionary<string, int>();
            var rnd = new Random();

            var (baseSets, baseReps, baseRest) = BasePrescription(onboarding.GoalType, onboarding.ExperienceLevel);

            for (int week = 1; week <= weeks; week++)
            {
                var (setsDelta, repsHint, deloadFactor) = WeekProgression(week, weeks);

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
                        Notes = $"Focus: {dayType.ToUpperInvariant()}" +
                                (string.IsNullOrWhiteSpace(repsHint) ? "" : $" • {repsHint}")
                    };
                    await sessionRepo.AddAsync(session);
                    await _uow.SaveAsync();

                    //chọn bài theo structure
                    var picks = new List<Workout>();

                    // Slot 1: Warmup/mobility
                    picks.Add(PickBest(
                        candidates,
                        w => tagIndex[w.Id].Contains("warmup") || tagIndex[w.Id].Contains("mobility"),
                        score: w =>
                        {
                            int s = 0;
                            if (tagIndex[w.Id].Contains("warmup")) s += 3;
                            if (tagIndex[w.Id].Contains("mobility")) s += 2;

                            if (requiredSafetyTags.Count > 0 && requiredSafetyTags.Except(tagIndex[w.Id]).Any())
                                s -= 3;

                            return s;
                        },
                        usedInPlan, week, rnd,
                        excludeIds: picks.Select(x => x.Id).ToHashSet()
                    ));

                    // Main block: 3-5 bài theo target
                    int mainCount = Math.Max(3, totalSlots - 2); // chừa 1 slot cho finshier
                    mainCount = Math.Min(mainCount, 5);

                    for (int i = 0; i < mainCount; i++)
                    {
                        picks.Add(PickBest(
                            candidates,
                            w => targets.Contains(w.PrimaryMuscle) || tagIndex[w.Id].Contains(dayType),
                            score: w =>
                            {
                                int s = 0;

                                // match muscle/plan day type
                                if (targets.Contains(w.PrimaryMuscle)) s += 6;
                                if (tagIndex[w.Id].Contains(dayType)) s += 3;

                                // focus bonus
                                if (focus.Contains("core") &&
                                    (w.PrimaryMuscle == MuscleGroup.Core || tagIndex[w.Id].Contains("core")))
                                    s += 2;

                                if (focus.Contains("glutes") &&
                                    (w.PrimaryMuscle == MuscleGroup.Glutes || tagIndex[w.Id].Contains("glutes")))
                                    s += 2;

                                // limitation penalty
                                if (requiredSafetyTags.Count > 0 && requiredSafetyTags.Except(tagIndex[w.Id]).Any())
                                    s -= 8;

                                // anti-repeat
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

                    // Finisher: cardio (LoseFat) hoặc core (GainMuscle/Strength)
                    bool wantCardioFinisher = onboarding.GoalType == GoalType.LoseFat;

                    picks.Add(PickBest(
                        candidates,
                        w => wantCardioFinisher
                            ? (w.PrimaryMuscle == MuscleGroup.Cardio || tagIndex[w.Id].Contains("cardio"))
                            : (w.PrimaryMuscle == MuscleGroup.Core || tagIndex[w.Id].Contains("core")),
                        score: w =>
                        {
                            int s = 0;

                            if (wantCardioFinisher &&
                                (w.PrimaryMuscle == MuscleGroup.Cardio || tagIndex[w.Id].Contains("cardio")))
                                s += 6;

                            if (!wantCardioFinisher &&
                                (w.PrimaryMuscle == MuscleGroup.Core || tagIndex[w.Id].Contains("core")))
                                s += 6;

                            if (requiredSafetyTags.Count > 0 && requiredSafetyTags.Except(tagIndex[w.Id]).Any())
                                s -= 8;

                            return s;
                        },
                        usedInPlan, week, rnd,
                        excludeIds: picks.Select(x => x.Id).ToHashSet()
                    ));

                    //lưu WorkoutSessionExercises
                    int order = 1;

                    foreach (var w in picks.Where(x => x != null))
                    {
                        usedInPlan[w.Id] = week;

                        var tags = tagIndex[w.Id];
                        var isCardio = w.PrimaryMuscle == MuscleGroup.Cardio || tags.Contains("cardio");
                        var isWarmup = tags.Contains("warmup") || tags.Contains("mobility");

                        int sets = isWarmup ? 1 : (int)Math.Max(1, Math.Round((baseSets + setsDelta) * deloadFactor));
                        string reps = isCardio ? "" : baseReps;
                        int rest = isWarmup ? 30 : baseRest;

                        int? durationSeconds = null;
                        if (isCardio)
                            durationSeconds = onboarding.MinutesPerSession <= 30 ? 180 : 240;

                        var ex = new WorkoutSessionExercise
                        {
                            WorkoutSessionId = session.Id,
                            WorkoutId = w.Id,
                            Order = order++,

                            Sets = isCardio ? null : sets,
                            Reps = isCardio ? null : reps,
                            DurationSeconds = durationSeconds,
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

        // Pick best
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
                var fb = candidates.FirstOrDefault(w => !excludeIds.Contains(w.Id));
                return fb!;
            }

            var pick = list[rnd.Next(Math.Min(4, list.Count))].w;
            return pick;
        }

        public async Task<object> GetPlanDetailAsync(string planId, string accountId)
        {
            var planRepo = _uow.GetRepository<WorkoutPlan>();

            var plan = await planRepo.Entities
                .Where(p => p.Id == planId && p.AccountId == accountId)
                .Include(p => p.WorkoutSchedules!)
                    .ThenInclude(w => w.WorkoutSessions!)
                        .ThenInclude(s => s.WorkoutSessionExercises!)
                            .ThenInclude(e => e.Workout)
                .FirstOrDefaultAsync();

            if (plan == null) throw new Exception("WorkoutPlan not found.");

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
                                s.Notes,
                                exercises = s.WorkoutSessionExercises!
                                    .OrderBy(e => e.Order)
                                    .Select(e => new
                                    {
                                        e.Order,
                                        e.Sets,
                                        e.Reps,
                                        e.DurationSeconds,
                                        e.RestSeconds,
                                        e.Note,
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
                throw new Exception("PlanId is required.");

            var planRepo = _uow.GetRepository<WorkoutPlan>();
            var plan = await planRepo.Entities
                .FirstOrDefaultAsync(p => p.Id == planId && p.AccountId == accountId);

            if (plan == null)
                throw new Exception("WorkoutPlan not found.");

            var scheduleRepo = _uow.GetRepository<WorkoutSchedule>();
            var scheduleIds = await scheduleRepo.Entities
                .Where(s => s.WorkoutPlanId == planId)
                .Select(s => s.Id)
                .ToListAsync();

            if (scheduleIds.Count > 0)
            {
                var sessionRepo = _uow.GetRepository<WorkoutSession>();
                var sessionIds = await sessionRepo.Entities
                    .Where(ss => scheduleIds.Contains(ss.WorkoutScheduleId))
                    .Select(ss => ss.Id)
                    .ToListAsync();

                if (sessionIds.Count > 0)
                {
                    var exRepo = _uow.GetRepository<WorkoutSessionExercise>();
                    var exercises = await exRepo.Entities
                        .Where(e => sessionIds.Contains(e.WorkoutSessionId))
                        .ToListAsync();

                    if (exercises.Count > 0)
                        exRepo.DeleteRange(exercises);

                    var sessions = await sessionRepo.Entities
                        .Where(ss => sessionIds.Contains(ss.Id))
                        .ToListAsync();

                    if (sessions.Count > 0)
                        sessionRepo.DeleteRange(sessions);
                }

                var schedules = await scheduleRepo.Entities
                    .Where(s => s.WorkoutPlanId == planId)
                    .ToListAsync();

                if (schedules.Count > 0)
                    scheduleRepo.DeleteRange(schedules);
            }

            planRepo.Delete(plan);
            await _uow.SaveAsync();
        }

        public async Task<IEnumerable<WorkoutPlanSummaryDto>> GetMyPlansAsync(string accountId)
        {
            var planRepo = _uow.GetRepository<WorkoutPlan>();

            return await planRepo.Entities
                .Where(p => p.AccountId == accountId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new WorkoutPlanSummaryDto
                {
                    Id = p.Id,
                    GoalType = p.GoalType,
                    Progress = p.Progress,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,

                    TotalWeeks = p.WorkoutSchedules!.Count(),
                    TotalSessions = p.WorkoutSchedules!.SelectMany(w => w.WorkoutSessions!).Count()
                })
                .ToListAsync();
        }

        public async Task<WorkoutPlanOverviewDto> GetPlanOverviewAsync(string planId, string accountId)
        {
            var planRepo = _uow.GetRepository<WorkoutPlan>();

            var data = await planRepo.Entities
                .Where(p => p.Id == planId && p.AccountId == accountId)
                .Select(p => new WorkoutPlanOverviewDto
                {
                    Id = p.Id,
                    GoalType = p.GoalType,
                    Progress = p.Progress,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Weeks = p.WorkoutSchedules!
                        .OrderBy(w => w.WeekNumber)
                        .Select(w => new WorkoutPlanWeekDto
                        {
                            ScheduleId = w.Id,
                            WeekNumber = w.WeekNumber,
                            Describe = w.Describe,
                            Progress = w.Progress,
                            TotalDays = w.WorkoutSessions!.Count()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (data == null) throw new Exception("WorkoutPlan not found.");
            return data;
        }

        public async Task<IEnumerable<WorkoutDayDto>> GetWeekDaysAsync(string planId, int weekNumber, string accountId)
        {
            // verify plan belongs to account + find that week schedule
            var scheduleRepo = _uow.GetRepository<WorkoutSchedule>();

            var scheduleId = await scheduleRepo.Entities
                .Where(s => s.WorkoutPlanId == planId && s.WeekNumber == weekNumber)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(scheduleId))
                throw new Exception("WorkoutSchedule (week) not found.");

            // check plan ownership
            var planRepo = _uow.GetRepository<WorkoutPlan>();
            var ok = await planRepo.Entities.AnyAsync(p => p.Id == planId && p.AccountId == accountId);
            if (!ok) throw new Exception("WorkoutPlan not found.");

            var sessionRepo = _uow.GetRepository<WorkoutSession>();

            return await sessionRepo.Entities
                .Where(s => s.WorkoutScheduleId == scheduleId)
                .OrderBy(s => s.DayNumber)
                .Select(s => new WorkoutDayDto
                {
                    SessionId = s.Id,
                    DayNumber = s.DayNumber,
                    Progress = s.Progress,
                    Notes = s.Notes,
                    ExerciseCount = s.WorkoutSessionExercises!.Count()
                })
                .ToListAsync();
        }

        public async Task<WorkoutSessionDetailDto> GetDayDetailAsync(string planId, int weekNumber, int dayNumber, string accountId)
        {
            // verify plan belongs to account
            var planRepo = _uow.GetRepository<WorkoutPlan>();
            var ok = await planRepo.Entities.AnyAsync(p => p.Id == planId && p.AccountId == accountId);
            if (!ok) throw new Exception("WorkoutPlan not found.");

            // find scheduleId by planId + weekNumber
            var scheduleRepo = _uow.GetRepository<WorkoutSchedule>();
            var scheduleId = await scheduleRepo.Entities
                .Where(s => s.WorkoutPlanId == planId && s.WeekNumber == weekNumber)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(scheduleId))
                throw new Exception("WorkoutSchedule (week) not found.");

            // load that day (with exercises + workout)
            var sessionRepo = _uow.GetRepository<WorkoutSession>();

            var session = await sessionRepo.Entities
                .Where(s => s.WorkoutScheduleId == scheduleId && s.DayNumber == dayNumber)
                .Include(s => s.WorkoutSessionExercises!)
                    .ThenInclude(e => e.Workout)
                .FirstOrDefaultAsync();

            if (session == null) throw new Exception("WorkoutSession (day) not found.");

            return new WorkoutSessionDetailDto
            {
                SessionId = session.Id,
                DayNumber = session.DayNumber,
                Notes = session.Notes,
                Progress = session.Progress,
                Exercises = session.WorkoutSessionExercises!
                    .OrderBy(e => e.Order)
                    .Select(e => new WorkoutSessionExerciseDto
                    {
                        Id= e.Id,
                        Order = e.Order,
                        Sets = e.Sets,
                        Reps = e.Reps,
                        IsDone = e.IsDone,                        
                        DurationSeconds = e.DurationSeconds,
                        RestSeconds = e.RestSeconds,
                        Note = e.Note,
                        Workout = new WorkoutMiniDto
                        {
                            Id = e.Workout!.Id,
                            Name = e.Workout.Name,
                            Describe = e.Workout.Describe,
                            InstructionVidLink = e.Workout.InstructionVidLink,
                            Level = e.Workout.Level,
                            Equipment = e.Workout.Equipment,
                            PrimaryMuscle = e.Workout.PrimaryMuscle,
                            Tags = e.Workout.Tags
                        }
                    })
                    .ToList()
            };
        }

        //-----Helpers
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

        private static (int setsDelta, string repsHint, double deloadFactor) WeekProgression(int week, int totalWeeks)
        {
            if (week == totalWeeks)
                return (-1, "deload", 0.75);

            var setsDelta = Math.Min(2, (week - 1) / 2);
            var repsHint = week >= 3 ? "+2 reps if easy" : "";
            return (setsDelta, repsHint, 1.0);
        }

        public async Task UpdateExerciseStatusAsync(string exerciseId, bool isDone, string accountId)
        {
            if (string.IsNullOrWhiteSpace(exerciseId))
                throw new Exception("ExerciseId is required.");

            var exRepo = _uow.GetRepository<WorkoutSessionExercise>();
            var sessionRepo = _uow.GetRepository<WorkoutSession>();
            var scheduleRepo = _uow.GetRepository<WorkoutSchedule>();
            var planRepo = _uow.GetRepository<WorkoutPlan>();

            var exercise = await exRepo.Entities
                .FirstOrDefaultAsync(e => e.Id == exerciseId);

            if (exercise == null)
                throw new Exception("WorkoutSessionExercise not found.");

            var session = await sessionRepo.Entities
                .FirstOrDefaultAsync(s => s.Id == exercise.WorkoutSessionId);

            if (session == null)
                throw new Exception("WorkoutSession not found.");

            var schedule = await scheduleRepo.Entities
                .FirstOrDefaultAsync(s => s.Id == session.WorkoutScheduleId);

            if (schedule == null)
                throw new Exception("WorkoutSchedule not found.");

            var plan = await planRepo.Entities
                .FirstOrDefaultAsync(p => p.Id == schedule.WorkoutPlanId && p.AccountId == accountId);

            if (plan == null)
                throw new Exception("WorkoutPlan not found.");

            exercise.IsDone = isDone;
            exRepo.Update(exercise);

            // recalc session progress
            var sessionExercises = await exRepo.Entities
                .Where(e => e.WorkoutSessionId == session.Id)
                .ToListAsync();

            session.Progress = CalculatePercent(
                sessionExercises.Count,
                sessionExercises.Count(e => e.Id == exercise.Id ? isDone : e.IsDone)
            );
            sessionRepo.Update(session);

            // recalc schedule progress
            var scheduleSessions = await sessionRepo.Entities
                .Where(s => s.WorkoutScheduleId == schedule.Id)
                .ToListAsync();

            schedule.Progress = CalculateAveragePercent(
                scheduleSessions.Select(s => s.Id == session.Id ? session.Progress : s.Progress)
            );
            scheduleRepo.Update(schedule);

            // recalc plan progress
            var planSchedules = await scheduleRepo.Entities
                .Where(s => s.WorkoutPlanId == plan.Id)
                .ToListAsync();

            plan.Progress = CalculateAveragePercent(
                planSchedules.Select(s => s.Id == schedule.Id ? schedule.Progress : s.Progress)
            );
            planRepo.Update(plan);

            await _uow.SaveAsync();
        }

        //private async Task RecalculateProgressAsync(string workoutSessionId, string workoutScheduleId, string workoutPlanId)
        //{
        //    var exRepo = _uow.GetRepository<WorkoutSessionExercise>();
        //    var sessionRepo = _uow.GetRepository<WorkoutSession>();
        //    var scheduleRepo = _uow.GetRepository<WorkoutSchedule>();
        //    var planRepo = _uow.GetRepository<WorkoutPlan>();

        //    // 1. Tính lại progress của WorkoutSession
        //    var sessionExercises = await exRepo.Entities
        //        .Where(e => e.WorkoutSessionId == workoutSessionId)
        //        .ToListAsync();

        //    var session = await sessionRepo.Entities
        //        .FirstOrDefaultAsync(s => s.Id == workoutSessionId);

        //    if (session == null)
        //        throw new Exception("WorkoutSession not found.");

        //    session.Progress = CalculatePercent(
        //        totalCount: sessionExercises.Count,
        //        doneCount: sessionExercises.Count(e => e.IsDone)
        //    );
        //    sessionRepo.Update(session);

        //    await _uow.SaveAsync();

        //    // 2. Tính lại progress của WorkoutSchedule
        //    var scheduleSessions = await sessionRepo.Entities
        //        .Where(s => s.WorkoutScheduleId == workoutScheduleId)
        //        .ToListAsync();

        //    var schedule = await scheduleRepo.Entities
        //        .FirstOrDefaultAsync(s => s.Id == workoutScheduleId);

        //    if (schedule == null)
        //        throw new Exception("WorkoutSchedule not found.");

        //    schedule.Progress = CalculateAveragePercent(scheduleSessions.Select(s => s.Progress));
        //    scheduleRepo.Update(schedule);

        //    await _uow.SaveAsync();

        //    // 3. Tính lại progress của WorkoutPlan
        //    var planSchedules = await scheduleRepo.Entities
        //        .Where(s => s.WorkoutPlanId == workoutPlanId)
        //        .ToListAsync();

        //    var plan = await planRepo.Entities
        //        .FirstOrDefaultAsync(p => p.Id == workoutPlanId);

        //    if (plan == null)
        //        throw new Exception("WorkoutPlan not found.");

        //    plan.Progress = CalculateAveragePercent(planSchedules.Select(s => s.Progress));
        //    planRepo.Update(plan);

        //    await _uow.SaveAsync();
        //}

        private static int CalculatePercent(int totalCount, int doneCount)
        {
            if (totalCount <= 0) return 0;

            return (int)Math.Round((double)doneCount * 100 / totalCount, MidpointRounding.AwayFromZero);
        }

        private static int CalculateAveragePercent(IEnumerable<int> values)
        {
            var list = values.ToList();
            if (list.Count == 0) return 0;

            return (int)Math.Round(list.Average(), MidpointRounding.AwayFromZero);
        }
    }
}
