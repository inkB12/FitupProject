using FitupProject.BLL.DTOs.AdminAccount;

namespace FitupProject.BLL.Interfaces
{
    public interface IAdminAccountService
    {
        Task<AccountListResponse> GetAccountsAsync(AccountListRequest request);
        Task<AccountResponse?> GetAccountByIdAsync(string id);
        Task<AccountResponse?> BanAccountAsync(string id);
        Task<AccountResponse?> UnbanAccountAsync(string id);
        Task<AccountResponse?> UpdateStatusAsync(string id, string status);
    }
}
