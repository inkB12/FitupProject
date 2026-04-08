using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class ConversionRate : BaseEntity
    {
        public ConversionRateType Type { get; set; }
        public decimal Rate { get; set; }

        public ConversionRateStatus Status { get; set; } = ConversionRateStatus.Active;

        // nav
        public ICollection<Payment>? Payments { get; set; }
    }
}
