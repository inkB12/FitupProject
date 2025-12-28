using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class WorkoutPlan : BaseEntity
    {
        public string AccountId { get; set; } = string.Empty;

        // link để trace plan tạo từ onboarding nào
        public string? OnboardingProfileId { get; set; }

        public GoalType GoalType { get; set; }
        public int Progress { get; set; } = 0;

        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        //public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // nav
        public Account? Account { get; set; }
        public OnboardingProfile? OnboardingProfile { get; set; }
        public ICollection<WorkoutSchedule>? WorkoutSchedules { get; set; }
    }
}
