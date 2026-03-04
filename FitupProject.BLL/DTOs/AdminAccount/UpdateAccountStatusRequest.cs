using System.ComponentModel.DataAnnotations;

namespace FitupProject.BLL.DTOs.AdminAccount
{
    public class UpdateAccountStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
