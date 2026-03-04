using System.ComponentModel.DataAnnotations;

namespace FitupProject.BLL.DTOs.PTRegister
{
    public class PTCertificationFileDto
    {
        [Required, MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string FileUrl { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ContentType { get; set; }

        public long? FileSize { get; set; }
    }

    public class PTRegisterRequest
    {
        [Required, MinLength(2), MaxLength(80)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Bio { get; set; }

        [Range(0, 60)]
        public int ExperienceYears { get; set; }

        [Range(0, 200000)]
        public int HourlyPointRate { get; set; }

        [MaxLength(120)]
        public string? Location { get; set; }

        // list string -> sẽ serialize JSON vào *Json fields
        public List<string>? Certifications { get; set; }
        public List<string>? Specialties { get; set; }
        public List<string>? Languages { get; set; }

        // tối đa 10
        public List<PTCertificationFileDto>? CertificationFiles { get; set; }
    }

    public class PTMeResponse
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
}
