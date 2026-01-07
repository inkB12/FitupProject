using FitupProject.BLL.DTOs.UserProfile;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        public UserProfilesController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserProfile([FromBody] UserProfileCreateRequest req)
        {
            UserProfileResponse response = await _userProfileService.CreateUserProfileAsync(req);
            return Ok(response);
        }

        [HttpGet] 
        public async Task<IActionResult> GetAllUserProfiles()
        {
            var profiles = await _userProfileService.GetAllUserProfilesAsync();
            return Ok(profiles);
        }

        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetUserProfileById(string accountId)
        {
            UserProfileResponse response = await _userProfileService.GetUserProfileByIdAsync(accountId);
            return Ok(response);
        }

        [HttpPut("{accountId}")]
        public async Task<IActionResult> UpdateUserProfile(string accountId, [FromBody] UserProfileUpdateRequest req)
        {
            await _userProfileService.UpdateUserProfileAsync(accountId, req);
            return NoContent();
        }
    }
}
