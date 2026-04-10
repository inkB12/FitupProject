using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class Account : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string PasswordHash { get; set; } = string.Empty;

        public decimal PointAmount { get; set; } = 0;

        public AccountStatus Status { get; set; } = AccountStatus.PendingVerification;
        public AccountRole Role { get; set; } = AccountRole.User;

        // OTP verify email
        public string? EmailOtpHash { get; set; }
        public DateTimeOffset? EmailOtpExpiresAt { get; set; }
        public int EmailOtpFailCount { get; set; } = 0;
        public DateTimeOffset? EmailVerifiedAt { get; set; }

        // OTP reset password
        public string? ResetPasswordOtpHash { get; set; }
        public DateTimeOffset? ResetPasswordOtpExpiresAt { get; set; }
        public int ResetPasswordOtpFailCount { get; set; } = 0;

        public UserProfile? UserProfile { get; set; }
        public PT? PT { get; set; }

        public ICollection<OnboardingProfile>? OnboardingProfiles { get; set; }
        public ICollection<WorkoutPlan>? WorkoutPlans { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<Premium>? Premiums { get; set; }
        public ICollection<AiConversation>? AiConversations { get; set; }
    }
}
