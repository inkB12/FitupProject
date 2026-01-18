using FitupProject.Core.Commons.Enums;

namespace FitupProject.BLL.DTOs.Accounts
{
    public class AccountCreateRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public AccountRole Role { get; set; } = AccountRole.User;
    }
}
