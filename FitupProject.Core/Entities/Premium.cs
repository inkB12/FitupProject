using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class Premium : BaseEntity
    {
        public string PremiumTypeId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;

        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public PremiumStatus Status { get; set; } = PremiumStatus.Active;

        // nav
        public PremiumType? PremiumType { get; set; }
        public Account? Account { get; set; }

        public ICollection<PremiumPayment>? PremiumPayments { get; set; }
    }
}
