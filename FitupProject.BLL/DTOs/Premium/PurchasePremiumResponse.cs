namespace FitupProject.BLL.DTOs.Premium
{
    public class PurchasePremiumResponse
    {
        public string PremiumId { get; set; } = string.Empty;
        public string PremiumTypeId { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public decimal Price { get; set; }

        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public decimal RemainingPointAmount { get; set; }

        public string ServicePaymentId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
