using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class OnboardingProfile : BaseEntity
    {
        public string AccountId { get; set; } = string.Empty;

        public GoalType GoalType { get; set; }
        public ExperienceLevel ExperienceLevel { get; set; }

        public int DaysPerWeek { get; set; }
        public int MinutesPerSession { get; set; }

        public EquipmentType Equipment { get; set; }

        // MVP: string lưu CSV hoặc JSON (vd: "Abs,Glutes")
        public string? FocusAreas { get; set; }
        public string? Limitations { get; set; }

        //public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // nav
        public Account? Account { get; set; }
        public ICollection<WorkoutPlan>? WorkoutPlans { get; set; }
    }
}
