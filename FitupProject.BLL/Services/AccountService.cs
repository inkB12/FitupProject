using FitupProject.BLL.DTOs.Accounts;
using FitupProject.BLL.DTOs.Me;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.BLL.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _uow;
        public AccountService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async  Task<string> CreateAccountAsync(AccountCreateRequest req)
        {
           if(string.IsNullOrWhiteSpace(req.Email))
            {
                throw new Exception("Email is required.");
            }
            if (string.IsNullOrWhiteSpace(req.Password))
            {
                throw new Exception("Password is required.");
            }
            var repo = _uow.GetRepository<Account>();
            var exists = await repo.Entities.AnyAsync(x => x.Email == req.Email);
            if (exists)
            {
                throw new Exception("Email already exists.");
            }
            var acc = new Account
            {
                Email = req.Email,
                PasswordHash = req.Password,
                Phone = req.Phone,
                Role = req.Role,
                Status = Core.Commons.Enums.AccountStatus.Active,
                PointAmount = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            await repo.AddAsync(acc);
            await _uow.SaveAsync();
            return acc.Id;
        }

        public async Task DeleteAccountAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new Exception("AccountId is required.");
            }
            var repo = _uow.GetRepository<Account>();
            var acc = await repo.Entities.FirstOrDefaultAsync(x => x.Id == id);
            if (acc == null)
            {
                throw new Exception("Account not found.");
            }
            repo.Delete(acc);
            await _uow.SaveAsync();
        }

        public async Task<Account?> GetAccountByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new Exception("AccountId is required.");
            }
            var repo = _uow.GetRepository<Account>();
            return await repo.Entities.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Account>> GetAccountsAsync()
        {
            var repo = _uow.GetRepository<Account>();
            return await repo.Entities.OrderByDescending(x => x.CreatedAt).ToListAsync();

        }

        public async Task<MeResponse> GetMeAsync(string accountId)
        {
            var repo = _uow.GetRepository<Account>();
            var acc = await repo.Entities
                .Include(x => x.UserProfile)
                .FirstOrDefaultAsync(x => x.Id == accountId);

            if (acc == null)
                throw new Exception("Account not found.");

            return new MeResponse
            {
                Id = acc.Id,
                Email = acc.Email,
                Phone = acc.Phone,
                Role = acc.Role.ToString(),
                Status = acc.Status.ToString(),
                PointAmount = acc.PointAmount,
                Profile = acc.UserProfile == null ? null : new MeProfileResponse
                {
                    FullName = acc.UserProfile.FullName,
                    Dob = acc.UserProfile.Dob,
                    Gender = acc.UserProfile.Gender,
                    Address = acc.UserProfile.Address,
                    Height = acc.UserProfile.Height,
                    Weight = acc.UserProfile.Weight
                }
            };
        }

        public async Task UpdateAccountAsync(string id, AccountUpdateRequest req)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new Exception("AccountId is required.");

            var repo = _uow.GetRepository<Account>();
            var acc = await repo.Entities.FirstOrDefaultAsync(x => x.Id == id);
            if (acc == null)
                throw new Exception("Account not found.");

            if (req.Phone != null)
                acc.Phone = req.Phone;

            if (req.Status.HasValue)
                acc.Status = req.Status.Value;

            if (req.Role.HasValue)
                acc.Role = req.Role.Value;

            if (req.PointAmount.HasValue)
                acc.PointAmount = req.PointAmount.Value;

            acc.UpdatedAt = DateTimeOffset.UtcNow;

            repo.Update(acc);
            await _uow.SaveAsync();
        }
    }
}
