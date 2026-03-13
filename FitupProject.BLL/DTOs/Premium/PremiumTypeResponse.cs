using FitupProject.Core.Commons.Enums;

namespace FitupProject.BLL.DTOs.Premium
{
    public class PremiumTypeResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Describe { get; set; } = string.Empty;
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public PremiumTypeStatus Status { get; set; }
    }
}
