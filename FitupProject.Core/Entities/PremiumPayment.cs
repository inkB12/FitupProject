using FitupProject.Core.Commons;

namespace FitupProject.Core.Entities
{
    public class PremiumPayment : BaseEntity
    {
        public string PremiumId { get; set; } = string.Empty;
        public string ServicePaymentId { get; set; } = string.Empty;

        public decimal Price { get; set; } // giá premium (point)

        // nav
        public Premium? Premium { get; set; }
        public ServicePayment? ServicePayment { get; set; }
    }
}
