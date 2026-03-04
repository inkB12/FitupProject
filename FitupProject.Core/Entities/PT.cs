using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class PT : BaseEntity
    {
        public string AccountId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;
        public string? Bio { get; set; }

        public decimal PricePerHour { get; set; }
        public decimal Rating { get; set; } = 0; // có thể tính trung bình từ review

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

        //New
        public int ExperienceYears { get; set; } = 0;

        public int HourlyPointRate { get; set; } = 0;

        public string? Location { get; set; }

        // store as JSON string to avoid heavy mapping/converters
        public string CertificationsJson { get; set; } = "[]";
        public string SpecialtiesJson { get; set; } = "[]";
        public string LanguagesJson { get; set; } = "[]";
        public DateTimeOffset SubmittedAt { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }       // reviewer AccountId
        public string? RejectedReason { get; set; }

        // nav
        public Account? Account { get; set; }
        public ICollection<Slot>? Slots { get; set; }
        public ICollection<PTCertificationFile>? CertificationFiles { get; set; }
        public ICollection<PTReviewLog>? ReviewLogs { get; set; }
    }
}
