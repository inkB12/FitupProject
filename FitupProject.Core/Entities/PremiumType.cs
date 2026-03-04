using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class PremiumType : BaseEntity
    {
        public string Describe { get; set; } = string.Empty;

        // số ngày
        public int Duration { get; set; }

        public decimal Price { get; set; } // giá bằng point
        public PremiumTypeStatus Status { get; set; } = PremiumTypeStatus.Active;

        // nav
        public ICollection<Premium>? Premiums { get; set; }
    }
}
