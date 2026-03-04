using System.ComponentModel.DataAnnotations;

namespace FitupProject.BLL.DTOs.PTRegister
{
    public class AdminPTListItemDto
    {
        public string PTId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public DateTimeOffset SubmittedAt { get; set; }
    }

    public class AdminPTDetailDto
    {
        public string PTId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;
        public string? Bio { get; set; }

        public int ExperienceYears { get; set; }
        public int HourlyPointRate { get; set; }
        public string? Location { get; set; }

        public List<string> Certifications { get; set; } = new();
        public List<string> Specialties { get; set; } = new();
        public List<string> Languages { get; set; } = new();

        public string VerificationStatus { get; set; } = string.Empty;

        public DateTimeOffset SubmittedAt { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }
        public string? RejectedReason { get; set; }

        public List<PTCertificationFileDto> CertificationFiles { get; set; } = new();
    }

    public class RejectPTRequest
    {
        [Required, MinLength(3), MaxLength(2000)]
        public string Reason { get; set; } = string.Empty;
    }
}
