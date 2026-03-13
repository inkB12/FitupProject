namespace FitupProject.BLL.DTOs.Premium
{
    public class MyPremiumStatusResponse
    {
        public bool HasPremium { get; set; }
        public bool IsActive { get; set; }

        public string? PremiumId { get; set; }
        public string? PremiumTypeId { get; set; }

        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        public int RemainingDays { get; set; }
    }
}
