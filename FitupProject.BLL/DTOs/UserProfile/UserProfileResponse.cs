using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitupProject.BLL.DTOs.UserProfile
{
    public class UserProfileResponse
    {
        public string AccountId { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
    }
}
