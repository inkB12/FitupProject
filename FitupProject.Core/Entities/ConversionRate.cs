using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class ConversionRate : BaseEntity
    {
        // ví dụ: 1 VND -> bao nhiêu point, hoặc 1 point -> bao nhiêu VND tuỳ bạn dùng
        public decimal Rate { get; set; }

        public ConversionRateStatus Status { get; set; } = ConversionRateStatus.Active;

        // nav
        public ICollection<Payment>? Payments { get; set; }
    }
}
