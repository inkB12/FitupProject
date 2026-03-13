using System.ComponentModel.DataAnnotations;

namespace FitupProject.BLL.DTOs.Premium
{
    public class CreatePremiumTypeRequest
    {
        [Required]
        public string Describe { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Duration { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
