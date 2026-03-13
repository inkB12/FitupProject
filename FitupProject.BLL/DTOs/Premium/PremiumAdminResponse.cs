using FitupProject.Core.Commons.Enums;

namespace FitupProject.BLL.DTOs.Premium
{
    public class PremiumAdminResponse
    {
        public string Id { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string PremiumTypeId { get; set; } = string.Empty;
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public PremiumStatus Status { get; set; }
    }
}
