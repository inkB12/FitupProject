using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitupProject.BLL.DTOs.UserProfile
{
    public class UserProfileUpdateRequest
    {
        public string? FullName { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
    }
}
