namespace FitupProject.BLL.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(string email, string password);
        Task VerifyOtpAsync(string email, string otp);
        Task ResendOtpAsync(string email);
        Task<string> LoginAsync(string email, string password);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(string email, string otp, string newPassword);

    }
}
