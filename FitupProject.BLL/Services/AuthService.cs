using FitupProject.BLL.Commons.Helpers;
using FitupProject.BLL.Commons.Securities;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Constants;
using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FitupProject.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _cfg;

        private readonly IJwtTokenGenerator _jwt;
        public AuthService(IUnitOfWork uow, IEmailSender emailSender, IConfiguration cfg, IJwtTokenGenerator jwt)
        {
            _uow = uow;
            _emailSender = emailSender;
            _cfg = cfg;
            _jwt = jwt;
        }


        public async Task RegisterAsync(string email, string password)
        {
            email = NormalizeEmail(email);

            var repo = _uow.GetRepository<Account>();

            var existing = await repo.Entities.FirstOrDefaultAsync(x => x.Email == email);

            if (existing != null)
            {
                if (existing.Status == AccountStatus.Active)
                    throw new Exception(MessageConstants.Auth.EmailAlreadyRegistered);

                await IssueAndSendOtpAsync(existing);
                repo.Update(existing); // OK vì existing là record đã có
                await _uow.SaveAsync();
                return;
            }

            var acc = new Account
            {
                Email = email,
                PasswordHash = PasswordHasher.Hash(password),
                Status = AccountStatus.PendingVerification,
                Role = AccountRole.User,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await IssueAndSendOtpAsync(acc);

            await repo.AddAsync(acc);


            await _uow.SaveAsync();
        }


        public async Task VerifyOtpAsync(string email, string otp)
        {
            email = NormalizeEmail(email);

            var repo = _uow.GetRepository<Account>();
            var acc = await repo.Entities.FirstOrDefaultAsync(x => x.Email == email);

            if (acc == null)
                throw new Exception(MessageConstants.Auth.InvalidEmailOrOtp);

            if (acc.Status == AccountStatus.Suspended)
                throw new Exception(MessageConstants.Auth.AccountSuspended);

            if (acc.Status == AccountStatus.Active)
                return; //đã active thì done

            if (string.IsNullOrWhiteSpace(acc.EmailOtpHash) || acc.EmailOtpExpiresAt == null)
                throw new Exception(MessageConstants.Auth.OtpNotFound);

            if (DateTimeOffset.UtcNow > acc.EmailOtpExpiresAt.Value)
                throw new Exception(MessageConstants.Auth.OtpExpired);

            if (acc.EmailOtpFailCount >= 5)
                throw new Exception(MessageConstants.Auth.TooManyFailedAttempts);

            var secret = GetOtpSecret();
            var inputHash = OtpHelper.HashOtp(otp.Trim(), secret);

            if (!OtpHelper.FixedEqualsBase64(acc.EmailOtpHash, inputHash))
            {
                acc.EmailOtpFailCount++;
                acc.UpdatedAt = DateTimeOffset.UtcNow;

                repo.Update(acc);
                await _uow.SaveAsync();

                throw new Exception(MessageConstants.Auth.InvalidEmailOrOtp);
            }

            // Success
            acc.Status = AccountStatus.Active;
            acc.EmailVerifiedAt = DateTimeOffset.UtcNow;

            acc.EmailOtpHash = null;
            acc.EmailOtpExpiresAt = null;
            acc.EmailOtpFailCount = 0;

            acc.UpdatedAt = DateTimeOffset.UtcNow;

            repo.Update(acc);
            await _uow.SaveAsync();
        }

        public async Task ResendOtpAsync(string email)
        {
            email = NormalizeEmail(email);

            var repo = _uow.GetRepository<Account>();
            var acc = await repo.Entities.FirstOrDefaultAsync(x => x.Email == email);

            if (acc == null) return;
            if (acc.Status == AccountStatus.Active) return;
            if (acc.Status == AccountStatus.Suspended) return;

            await IssueAndSendOtpAsync(acc);

            repo.Update(acc);
            await _uow.SaveAsync();
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            email = NormalizeEmail(email);
            var repo = _uow.GetRepository<Account>();

            var acc = await repo.Entities.FirstOrDefaultAsync(x => x.Email == email);
            if (acc == null)
                throw new Exception(MessageConstants.Auth.InvalidCredentials);

            if (acc.Status == AccountStatus.Suspended)
                throw new Exception(MessageConstants.Auth.AccountSuspended);

            if (acc.Status != AccountStatus.Active)
                throw new Exception(MessageConstants.Auth.AccountNotActive);

            if (!PasswordHasher.Verify(password, acc.PasswordHash))
                throw new Exception(MessageConstants.Auth.InvalidCredentials);

            return _jwt.Generate(acc);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            email = NormalizeEmail(email);
            var repo = _uow.GetRepository<Account>();

            var acc = await repo.Entities.FirstOrDefaultAsync(x => x.Email == email);

            // chống lộ email tồn tại: luôn return OK
            if (acc == null) return;
            if (acc.Status == AccountStatus.Suspended) return;

            var otp = OtpHelper.Generate6();
            var secret = GetOtpSecret();

            acc.ResetPasswordOtpHash = OtpHelper.HashOtp(otp, secret);
            acc.ResetPasswordOtpExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
            acc.ResetPasswordOtpFailCount = 0;
            acc.UpdatedAt = DateTimeOffset.UtcNow;

            repo.Update(acc);
            await _uow.SaveAsync();

            var subject = "FitUp - Reset your account";
            var body = EmailTemplate.LoadOtpTemplate(otp, 30);
            await _emailSender.SendAsync(acc.Email, subject, body);
        }

        public async Task ResetPasswordAsync(string email, string otp, string newPassword)
        {
            email = NormalizeEmail(email);
            otp = (otp ?? "").Trim();

            var repo = _uow.GetRepository<Account>();
            var acc = await repo.Entities.FirstOrDefaultAsync(x => x.Email == email);

            // vẫn trả 400 message chung để bảo mật
            if (acc == null)
                throw new Exception(MessageConstants.Auth.InvalidResetOtp);

            if (acc.Status == AccountStatus.Suspended)
                throw new Exception(MessageConstants.Auth.AccountSuspended);

            if (string.IsNullOrWhiteSpace(acc.ResetPasswordOtpHash) || acc.ResetPasswordOtpExpiresAt == null)
                throw new Exception(MessageConstants.Auth.InvalidResetOtp);

            if (DateTimeOffset.UtcNow > acc.ResetPasswordOtpExpiresAt.Value)
                throw new Exception(MessageConstants.Auth.ResetOtpExpired);

            if (acc.ResetPasswordOtpFailCount >= 5)
                throw new Exception(MessageConstants.Auth.InvalidResetOtp);

            var secret = GetOtpSecret();
            var inputHash = OtpHelper.HashOtp(otp, secret);

            if (!OtpHelper.FixedEqualsBase64(acc.ResetPasswordOtpHash, inputHash))
            {
                acc.ResetPasswordOtpFailCount++;
                acc.UpdatedAt = DateTimeOffset.UtcNow;
                repo.Update(acc);
                await _uow.SaveAsync();

                throw new Exception(MessageConstants.Auth.InvalidResetOtp);
            }

            // OK -> đổi password
            acc.PasswordHash = PasswordHasher.Hash(newPassword);

            // clear otp
            acc.ResetPasswordOtpHash = null;
            acc.ResetPasswordOtpExpiresAt = null;
            acc.ResetPasswordOtpFailCount = 0;
            acc.UpdatedAt = DateTimeOffset.UtcNow;

            repo.Update(acc);
            await _uow.SaveAsync();
        }



        private async Task IssueAndSendOtpAsync(Account acc)
        {
            var otp = OtpHelper.Generate6();
            var secret = GetOtpSecret();

            acc.EmailOtpHash = OtpHelper.HashOtp(otp, secret);
            acc.EmailOtpExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
            acc.EmailOtpFailCount = 0;
            acc.UpdatedAt = DateTimeOffset.UtcNow;

            var subject = "FitUp - Verify your account";
            var body = EmailTemplate.LoadOtpTemplate(otp, 30);
            await _emailSender.SendAsync(acc.Email, subject, body);
        }

        private string GetOtpSecret()
        {
            var secret = _cfg["Otp:Secret"];
            if (string.IsNullOrWhiteSpace(secret))
                throw new Exception("Missing config: Otp:Secret");
            return secret;
        }

        private static string NormalizeEmail(string email)
            => (email ?? string.Empty).Trim().ToLowerInvariant();
    }
}
