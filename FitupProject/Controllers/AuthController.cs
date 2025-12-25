using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Constants;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        public record RegisterRequest(string Email, string Password);
        public record VerifyOtpRequest(string Email, string Otp);
        public record ResendOtpRequest(string Email);
        public record LoginRequest(string Email, string Password);
        public record LoginResponse(string AccessToken);
        public record ForgotPasswordRequest(string Email);
        public record ResetPasswordRequest(string Email, string Otp, string NewPassword);

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest req)
        {
            await _auth.RegisterAsync(req.Email, req.Password);
            return Ok(new { message = "OTP has been sent." });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequest req)
        {
            await _auth.VerifyOtpAsync(req.Email, req.Otp);
            return Ok(new { message = "Account activated." });
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp(ResendOtpRequest req)
        {
            await _auth.ResendOtpAsync(req.Email);
            return Ok(new { message = "OTP has been sent." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            var token = await _auth.LoginAsync(req.Email, req.Password);
            return Ok(new LoginResponse(token));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
        {
            await _auth.ForgotPasswordAsync(req.Email);
            return Ok(new { message = MessageConstants.Auth.ForgotPasswordOtpSent });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest req)
        {
            await _auth.ResetPasswordAsync(req.Email, req.Otp, req.NewPassword);
            return Ok(new { message = MessageConstants.Auth.ResetPasswordSuccess });
        }

    }
}
