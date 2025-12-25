using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitupProject.Core.Entities
{
    public class UserProfile
    {
        [Key]
        public string AccountId { get; set; } = string.Empty;

        public string? FullName { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }

        [ForeignKey(nameof(AccountId))]
        public Account? Account { get; set; }
    }
}
