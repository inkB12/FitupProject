using FitupProject.Core.Commons.Enums;

namespace FitupProject.BLL.DTOs.PTs
{
    public class PTProfileResponse
    {
        public string Id { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public decimal PricePerHour { get; set; }
        public decimal Rating { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
    }

    public class PTListItemResponse
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public decimal PricePerHour { get; set; }
        public decimal Rating { get; set; }
    }

    public class PTFilterRequest
    {
        public string? Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
