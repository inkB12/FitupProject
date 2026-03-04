using FitupProject.BLL.DTOs.Accounts;
using FitupProject.Core.Entities;

namespace FitupProject.BLL.Interfaces
{
    public interface IAccountService
    {
        Task<List<Account>> GetAccountsAsync();
        Task<Account?> GetAccountByIdAsync(string id);
        Task<string> CreateAccountAsync(AccountCreateRequest req);
        Task UpdateAccountAsync(string id, AccountUpdateRequest req);
        Task DeleteAccountAsync(string id);
    }
}
