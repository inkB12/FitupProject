namespace FitupProject.Core.Commons.Constants
{
    public static class MessageConstants
    {
        // ===== Auth / Account =====
        public static class Auth
        {
            public const string RegisterOtpSent = "If the email is valid, OTP has been sent.";
            public const string AccountActivated = "Account activated.";
            public const string EmailAlreadyRegistered = "Email already registered.";
            public const string InvalidEmailOrOtp = "Invalid email or OTP.";
            public const string OtpNotFound = "OTP not found. Please resend OTP.";
            public const string OtpExpired = "OTP expired. Please resend OTP.";
            public const string TooManyFailedAttempts = "Too many failed attempts. Please resend OTP.";
            public const string AccountSuspended = "Account is suspended.";
            public const string InvalidCredentials = "Email or password is incorrect.";
            public const string AccountNotActive = "Account is not activated. Please verify OTP.";
            public const string ForgotPasswordOtpSent = "If the email exists, OTP has been sent.";
            public const string InvalidResetOtp = "Invalid OTP.";
            public const string ResetOtpExpired = "OTP expired. Please resend OTP.";
            public const string ResetPasswordSuccess = "Password has been reset successfully.";
        }

        // ===== Common =====
        public static class Common
        {
            public const string Success = "Success.";
            public const string BadRequest = "Bad request.";
            public const string NotFound = "Not found.";
        }
    }
}
