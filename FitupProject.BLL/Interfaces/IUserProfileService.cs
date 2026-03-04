using FitupProject.BLL.DTOs.UserProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitupProject.BLL.Interfaces
{
    public interface IUserProfileService
    {
        // create
        Task<UserProfileResponse> CreateUserProfileAsync(UserProfileCreateRequest req);
        // read
        Task<UserProfileResponse> GetUserProfileByIdAsync(string accountId);
        Task<List<UserProfileResponse>> GetAllUserProfilesAsync();
        // update
        Task UpdateUserProfileAsync(string accountId, UserProfileUpdateRequest req);
    }
}
