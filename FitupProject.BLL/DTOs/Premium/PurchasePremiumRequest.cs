using System.ComponentModel.DataAnnotations;

namespace FitupProject.BLL.DTOs.Premium
{
    public class PurchasePremiumRequest
    {
        [Required]
        public string PremiumTypeId { get; set; } = string.Empty;
    }
}
