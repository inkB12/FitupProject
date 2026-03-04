using FitupProject.BLL.DTOs.UserProfile;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace FitupProject.BLL.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUnitOfWork _uow;
        public UserProfileService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<UserProfileResponse> CreateUserProfileAsync(UserProfileCreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.AccountId))
            {
                throw new Exception("AccountId is required.");
            }

            var accountRepo = _uow.GetRepository<Account>();
            bool accountExist = await accountRepo.Entities.AnyAsync(x => x.Id == req.AccountId);
            if(!accountExist)
            {
                throw new Exception("AccountId does not exist.");
            }   

            var userProfileRepo = _uow.GetRepository<UserProfile>();
            bool exist = await userProfileRepo.Entities.AnyAsync(x => x.AccountId == req.AccountId);
            if(exist)
            {
                throw new Exception("UserProfile for this AccountId already exists.");
            }

            UserProfile profile = new UserProfile
            {
                AccountId = req.AccountId,
                FullName = req.FullName?.Trim(),
                Dob = req.Dob,
                Gender = req.Gender?.Trim(),
                Address = req.Address?.Trim(),
                Height = req.Height,
                Weight = req.Weight
            };

            await userProfileRepo.AddAsync(profile);
            await _uow.SaveAsync();

            return new UserProfileResponse
            {
                AccountId = profile.AccountId,
                FullName = profile.FullName,
                Dob = profile.Dob,
                Gender = profile.Gender,
                Address = profile.Address,
                Height = profile.Height,
                Weight = profile.Weight,
            };
        }

        public async Task<List<UserProfileResponse>> GetAllUserProfilesAsync()
        {
            var repo = _uow.GetRepository<UserProfile>();
            var profiles = repo.Entities.Select(x => new UserProfileResponse
            {
                AccountId = x.AccountId,
                FullName = x.FullName,
                Dob = x.Dob,
                Gender = x.Gender,
                Address = x.Address,
                Height = x.Height,
                Weight = x.Weight,
            });
            return await profiles.ToListAsync();
        }

        public async Task<UserProfileResponse> GetUserProfileByIdAsync(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new Exception("AccountId is required.");

            var repo = _uow.GetRepository<UserProfile>();

            var response = await repo.Entities
                .Where(x => x.AccountId == accountId)
                .Select(x => new UserProfileResponse
                {
                    AccountId = x.AccountId,
                    FullName = x.FullName,
                    Dob = x.Dob,
                    Gender = x.Gender,
                    Address = x.Address,
                    Height = x.Height,
                    Weight = x.Weight
                })
                .FirstOrDefaultAsync();

            if (response == null)
                throw new Exception("UserProfile not found.");

            return response;
        }

        public async Task UpdateUserProfileAsync(string accountId, UserProfileUpdateRequest req)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new Exception("AccountId is required.");

            var repo = _uow.GetRepository<UserProfile>();

            var profile = await repo.GetByIdAsync(accountId);

            if (profile == null)
                throw new Exception("UserProfile not found.");
            if (req.FullName != null)
                profile.FullName = req.FullName.Trim();
            if (req.Dob.HasValue)
                profile.Dob = req.Dob;
            if (req.Gender != null)
                profile.Gender = req.Gender.Trim();
            if (req.Address != null)
                profile.Address = req.Address.Trim();
            if (req.Height.HasValue)
                profile.Height = req.Height;
            if (req.Weight.HasValue)
                profile.Weight = req.Weight;

            await repo.UpdateAsync(profile);
            await _uow.SaveAsync();
        }
    }
}
