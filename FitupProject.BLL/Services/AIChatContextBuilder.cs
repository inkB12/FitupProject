using System.Text;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.BLL.Services
{
    public class AIChatContextBuilder : IAIChatContextBuilder
    {
        private readonly IUnitOfWork _uow;

        public AIChatContextBuilder(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<string> BuildContextAsync(string accountId)
        {
            var sb = new StringBuilder();

            var utcNow = DateTimeOffset.UtcNow;
            var vnNow = ConvertUtcToVietnam(utcNow);
            var todayVn = DateOnly.FromDateTime(vnNow.DateTime);

            var account = await _uow.GetRepository<Account>().Entities
                .AsNoTracking()
                .Include(x => x.UserProfile)
                .FirstOrDefaultAsync(x => x.Id == accountId && x.DeleteAt == null);

            if (account == null)
                return string.Empty;

            sb.AppendLine("FITUP LIVE USER CONTEXT");

            sb.AppendLine("=== ACCOUNT ===");
            sb.AppendLine($"- AccountId: {account.Id}");
            sb.AppendLine($"- Email: {Safe(account.Email)}");
            sb.AppendLine($"- Role: {account.Role}");
            sb.AppendLine($"- AccountStatus: {account.Status}");
            sb.AppendLine($"- PointAmount: {account.PointAmount}");

            if (account.UserProfile != null)
            {
                var p = account.UserProfile;
                sb.AppendLine("=== USER PROFILE ===");
                sb.AppendLine($"- FullName: {Safe(p.FullName)}");
                sb.AppendLine($"- Dob: {FormatDate(p.Dob)}");
                sb.AppendLine($"- Gender: {Safe(p.Gender)}");
                sb.AppendLine($"- Address: {Safe(p.Address)}");
                sb.AppendLine($"- Height: {(p.Height.HasValue ? $"{p.Height.Value} cm" : "N/A")}");
                sb.AppendLine($"- Weight: {(p.Weight.HasValue ? $"{p.Weight.Value} kg" : "N/A")}");
            }

            var onboarding = await _uow.GetRepository<OnboardingProfile>().Entities
                .AsNoTracking()
                .Where(x => x.AccountId == accountId && x.DeleteAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (onboarding != null)
            {
                sb.AppendLine("=== LATEST ONBOARDING ===");
                sb.AppendLine($"- GoalType: {onboarding.GoalType}");
                sb.AppendLine($"- ExperienceLevel: {onboarding.ExperienceLevel}");
                sb.AppendLine($"- Weeks: {onboarding.Weeks}");
                sb.AppendLine($"- DaysPerWeek: {onboarding.DaysPerWeek}");
                sb.AppendLine($"- MinutesPerSession: {onboarding.MinutesPerSession}");
                sb.AppendLine($"- Equipment: {onboarding.Equipment}");
                sb.AppendLine($"- FocusAreas: {Crop(onboarding.FocusAreas, 120)}");
                sb.AppendLine($"- Limitations: {Crop(onboarding.Limitations, 120)}");
            }

            var premium = await _uow.GetRepository<Premium>().Entities
                .AsNoTracking()
                .Include(x => x.PremiumType)
                .Where(x => x.AccountId == accountId && x.DeleteAt == null)
                .OrderByDescending(x => x.StartDate <= utcNow && x.EndDate >= utcNow)
                .ThenByDescending(x => x.EndDate)
                .ThenByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (premium != null)
            {
                var isActivePremium = premium.StartDate <= utcNow && premium.EndDate >= utcNow;

                sb.AppendLine("=== PREMIUM ===");
                sb.AppendLine($"- IsActiveNow: {isActivePremium}");
                sb.AppendLine($"- Status: {premium.Status}");
                sb.AppendLine($"- StartDate: {FormatDateTimeOffset(premium.StartDate)}");
                sb.AppendLine($"- EndDate: {FormatDateTimeOffset(premium.EndDate)}");

                if (premium.PremiumType != null)
                {
                    sb.AppendLine($"- PremiumDurationDays: {premium.PremiumType.Duration}");
                    sb.AppendLine($"- PremiumPricePoint: {premium.PremiumType.Price}");
                    sb.AppendLine($"- PremiumDescription: {Crop(premium.PremiumType.Describe, 120)}");
                }
            }

            var workoutPlan = await _uow.GetRepository<WorkoutPlan>().Entities
                .AsNoTracking()
                .Include(x => x.WorkoutSchedules)!
                    .ThenInclude(x => x.WorkoutSessions)!
                        .ThenInclude(x => x.WorkoutSessionExercises)!
                            .ThenInclude(x => x.Workout)
                .Where(x => x.AccountId == accountId && x.DeleteAt == null)
                .OrderByDescending(x => x.StartDate <= utcNow && x.EndDate >= utcNow)
                .ThenByDescending(x => x.StartDate)
                .ThenByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (workoutPlan != null)
            {
                var isActivePlan = workoutPlan.StartDate <= utcNow && workoutPlan.EndDate >= utcNow;

                var allSchedules = workoutPlan.WorkoutSchedules?
                    .OrderBy(x => x.WeekNumber)
                    .ToList() ?? new List<WorkoutSchedule>();

                var allSessions = allSchedules
                    .SelectMany(x => x.WorkoutSessions ?? Enumerable.Empty<WorkoutSession>())
                    .OrderBy(x => x.StartDate ?? DateTimeOffset.MaxValue)
                    .ThenBy(x => x.DayNumber)
                    .ToList();

                var nextSession = allSessions
                    .Where(x => !x.EndDate.HasValue || x.EndDate.Value >= utcNow.AddDays(-1))
                    .OrderBy(x => x.StartDate ?? DateTimeOffset.MaxValue)
                    .ThenBy(x => x.DayNumber)
                    .FirstOrDefault();

                sb.AppendLine("=== WORKOUT PLAN ===");
                sb.AppendLine($"- IsActiveNow: {isActivePlan}");
                sb.AppendLine($"- GoalType: {workoutPlan.GoalType}");
                sb.AppendLine($"- Progress: {workoutPlan.Progress}%");
                sb.AppendLine($"- StartDate: {FormatDateTimeOffset(workoutPlan.StartDate)}");
                sb.AppendLine($"- EndDate: {FormatDateTimeOffset(workoutPlan.EndDate)}");
                sb.AppendLine($"- ScheduleCount: {allSchedules.Count}");
                sb.AppendLine($"- SessionCount: {allSessions.Count}");

                if (nextSession != null)
                {
                    sb.AppendLine("=== NEXT WORKOUT SESSION ===");
                    sb.AppendLine($"- DayNumber: {nextSession.DayNumber}");
                    sb.AppendLine($"- Progress: {nextSession.Progress}%");
                    sb.AppendLine($"- StartDate: {FormatNullableDateTimeOffset(nextSession.StartDate)}");
                    sb.AppendLine($"- EndDate: {FormatNullableDateTimeOffset(nextSession.EndDate)}");
                    sb.AppendLine($"- Notes: {Crop(nextSession.Notes, 120)}");

                    var exercises = nextSession.WorkoutSessionExercises?
                        .OrderBy(x => x.Order)
                        .Take(5)
                        .Select(FormatExercise)
                        .ToList() ?? new List<string>();

                    if (exercises.Count > 0)
                    {
                        sb.AppendLine("- Exercises:");
                        foreach (var ex in exercises)
                        {
                            sb.AppendLine($"  • {ex}");
                        }
                    }
                }
            }

            var upcomingBooking = await _uow.GetRepository<Booking>().Entities
                .AsNoTracking()
                .Include(x => x.SlotForBooking)!
                    .ThenInclude(x => x.Slot)!
                        .ThenInclude(x => x.PT)
                .Where(x =>
                    x.AccountId == accountId &&
                    x.DeleteAt == null &&
                    x.SlotForBooking != null &&
                    x.SlotForBooking.BookingDate >= todayVn)
                .OrderBy(x => x.SlotForBooking!.BookingDate)
                .ThenBy(x => x.SlotForBooking!.Slot!.SlotStart)
                .FirstOrDefaultAsync();

            if (upcomingBooking != null && upcomingBooking.SlotForBooking?.Slot?.PT != null)
            {
                var slot = upcomingBooking.SlotForBooking;
                var pt = slot.Slot!.PT!;

                sb.AppendLine("=== UPCOMING PT BOOKING ===");
                sb.AppendLine($"- BookingStatus: {upcomingBooking.Status}");
                sb.AppendLine($"- BookingDate: {slot.BookingDate:yyyy-MM-dd}");
                sb.AppendLine($"- Time: {slot.Slot.SlotStart:HH\\:mm} - {slot.Slot.SlotEnd:HH\\:mm}");
                sb.AppendLine($"- Price: {slot.Price}");
                sb.AppendLine($"- PTName: {Safe(pt.DisplayName)}");
                sb.AppendLine($"- PTLocation: {Safe(pt.Location)}");
                sb.AppendLine($"- PTPricePerHour: {pt.PricePerHour}");
                sb.AppendLine($"- PTRating: {pt.Rating}");
                sb.AppendLine($"- Note: {Crop(upcomingBooking.Note, 120)}");
            }

            var latestPayment = await _uow.GetRepository<Payment>().Entities
                .AsNoTracking()
                .Where(x => x.AccountId == accountId && x.DeleteAt == null)
                .OrderByDescending(x => x.PaidAt ?? x.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestPayment != null)
            {
                sb.AppendLine("=== LATEST TOPUP PAYMENT ===");
                sb.AppendLine($"- AmountVnd: {latestPayment.Amount}");
                sb.AppendLine($"- Status: {latestPayment.Status}");
                sb.AppendLine($"- Method: {latestPayment.Method}");
                sb.AppendLine($"- PaidAt: {FormatNullableDateTimeOffset(latestPayment.PaidAt)}");
                sb.AppendLine($"- ExpiredAt: {FormatNullableDateTimeOffset(latestPayment.ExpiredAt)}");
            }

            return sb.ToString().Trim();
        }

        private static string FormatExercise(WorkoutSessionExercise x)
        {
            var name = x.Workout?.Name ?? $"WorkoutId:{x.WorkoutId}";
            var pieces = new List<string> { name };

            if (x.Sets.HasValue) pieces.Add($"{x.Sets.Value} sets");
            if (!string.IsNullOrWhiteSpace(x.Reps)) pieces.Add($"{x.Reps} reps");
            if (x.DurationSeconds.HasValue) pieces.Add($"{x.DurationSeconds.Value}s");
            if (x.RestSeconds.HasValue) pieces.Add($"rest {x.RestSeconds.Value}s");
            if (!string.IsNullOrWhiteSpace(x.Note)) pieces.Add(Crop(x.Note, 50));

            return string.Join(" | ", pieces);
        }

        private static string Safe(string? value)
            => string.IsNullOrWhiteSpace(value) ? "N/A" : value.Trim();

        private static string Crop(string? value, int max)
        {
            if (string.IsNullOrWhiteSpace(value)) return "N/A";
            var clean = value.Trim().Replace("\r", " ").Replace("\n", " ");
            if (clean.Length <= max) return clean;
            return clean[..max] + "...";
        }

        private static string FormatDate(DateTime? dt)
            => dt.HasValue ? dt.Value.ToString("yyyy-MM-dd") : "N/A";

        private static string FormatDateTimeOffset(DateTimeOffset dt)
            => ConvertUtcToVietnam(dt).ToString("yyyy-MM-dd HH:mm:ss");

        private static string FormatNullableDateTimeOffset(DateTimeOffset? dt)
            => dt.HasValue ? ConvertUtcToVietnam(dt.Value).ToString("yyyy-MM-dd HH:mm:ss") : "N/A";

        private static DateTimeOffset ConvertUtcToVietnam(DateTimeOffset utcValue)
        {
            var tz = GetVietnamTimeZone();
            return TimeZoneInfo.ConvertTime(utcValue, tz);
        }

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
        }
    }
}
