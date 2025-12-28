using FitupProject.Core.Commons.Enums;

namespace FitupProject.BLL.DTOs.Onboarding
{
    public class OnboardingSubmitRequest
    {
        public GoalType GoalType { get; set; }
        public ExperienceLevel ExperienceLevel { get; set; }
        public int Weeks { get; set; } = 4;
        public int DaysPerWeek { get; set; }           // 3..6
        public int MinutesPerSession { get; set; }     // 15/30/45/60
        public EquipmentType Equipment { get; set; }

        public string? FocusAreas { get; set; }        // CSV: "Core,Glutes"
        public string? Limitations { get; set; }       // CSV: "knee,back"
    }
}
