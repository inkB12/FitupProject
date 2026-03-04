using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class PT : BaseEntity
    {
        public string AccountId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;
        public string? Bio { get; set; }

        public decimal PricePerHour { get; set; }
        public decimal Rating { get; set; } = 0; // có thể tính trung bình từ review

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

        // nav
        public Account? Account { get; set; }
        public ICollection<Slot>? Slots { get; set; }
    }
}
