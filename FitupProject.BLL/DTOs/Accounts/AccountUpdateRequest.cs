using FitupProject.Core.Commons.Enums;

namespace FitupProject.BLL.DTOs.Accounts
{
    public class AccountUpdateRequest
    {
        public string? Phone { get; set; }
        public AccountStatus? Status { get; set; }
        public AccountRole? Role { get; set; }
        public decimal? PointAmount { get; set; }
    }
}
