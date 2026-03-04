using FitupProject.BLL.DTOs.AdminAccount;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.BLL.Services
{
    public class AdminAccountService : IAdminAccountService
    {
        private readonly IUnitOfWork _uow;

        public AdminAccountService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<AccountListResponse> GetAccountsAsync(AccountListRequest request)
        {
            var repo = _uow.GetRepository<Account>();
            var query = repo.Entities.AsQueryable();

            // Filter by search (email or phone)
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.Email.ToLower().Contains(search) ||
                                         (x.Phone != null && x.Phone.Contains(search)));
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<AccountStatus>(request.Status, true, out var status))
            {
                query = query.Where(x => x.Status == status);
            }

            // Filter by role
            if (!string.IsNullOrWhiteSpace(request.Role) &&
                Enum.TryParse<AccountRole>(request.Role, true, out var role))
            {
                query = query.Where(x => x.Role == role);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => MapToResponse(x))
                .ToListAsync();

            return new AccountListResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<AccountResponse?> GetAccountByIdAsync(string id)
        {
            var repo = _uow.GetRepository<Account>();
            var account = await repo.Entities.FirstOrDefaultAsync(x => x.Id == id);

            return account == null ? null : MapToResponse(account);
        }

        public async Task<AccountResponse?> BanAccountAsync(string id)
        {
            return await UpdateStatusAsync(id, AccountStatus.Suspended.ToString());
        }

        public async Task<AccountResponse?> UnbanAccountAsync(string id)
        {
            return await UpdateStatusAsync(id, AccountStatus.Active.ToString());
        }

        public async Task<AccountResponse?> UpdateStatusAsync(string id, string status)
        {
            if (!Enum.TryParse<AccountStatus>(status, true, out var newStatus))
                throw new ArgumentException($"Invalid status: {status}");

            var repo = _uow.GetRepository<Account>();
            var account = await repo.Entities.FirstOrDefaultAsync(x => x.Id == id);

            if (account == null)
                return null;

            account.Status = newStatus;
            account.UpdatedAt = DateTimeOffset.UtcNow;

            repo.Update(account);
            await _uow.SaveAsync();

            return MapToResponse(account);
        }

        private static AccountResponse MapToResponse(Account account)
        {
            return new AccountResponse
            {
                Id = account.Id,
                Email = account.Email,
                Phone = account.Phone,
                Status = account.Status.ToString(),
                Role = account.Role.ToString(),
                PointAmount = account.PointAmount,
                EmailVerifiedAt = account.EmailVerifiedAt,
                CreatedAt = account.CreatedAt,
                UpdatedAt = account.UpdatedAt
            };
        }
    }
}
