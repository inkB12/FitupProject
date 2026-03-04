using FitupProject.BLL.Commons.Exceptions;
using FitupProject.BLL.DTOs.Accounts;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                throw new ExceptionHandler("Email is required.");
            }
            if (string.IsNullOrWhiteSpace(req.Password))
            {
                throw new ExceptionHandler("Password is required.");
            }
            var repo = _uow.GetRepository<Account>();
            var exists = await repo.Entities.AnyAsync(x => x.Email == req.Email);
            if (exists)
            {
                throw new ExceptionHandler("Email already exists.");
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
                throw new ExceptionHandler("AccountId is required.");
            }
            var repo = _uow.GetRepository<Account>();
            var acc = await repo.Entities.FirstOrDefaultAsync(x => x.Id == id);
            if (acc == null)
            {
                throw new ExceptionHandler("Account not found.");
            }
            repo.Delete(acc);
            await _uow.SaveAsync();
        }

        public async Task<Account?> GetAccountByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ExceptionHandler("AccountId is required.");
            }
            var repo = _uow.GetRepository<Account>();
            return await repo.Entities.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Account>> GetAccountsAsync()
        {
            var repo = _uow.GetRepository<Account>();
            return await repo.Entities.OrderByDescending(x => x.CreatedAt).ToListAsync();

        }

        public async Task UpdateAccountAsync(string id, AccountUpdateRequest req)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ExceptionHandler("AccountId is required.");

            var repo = _uow.GetRepository<Account>();
            var acc = await repo.Entities.FirstOrDefaultAsync(x => x.Id == id);
            if (acc == null)
                throw new ExceptionHandler("Account not found.");

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
