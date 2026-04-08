using FitupProject.BLL.DTOs.Payments;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using FitupPayOSOptions = FitupProject.BLL.Commons.PayOS.PayOSOptions;

namespace FitupProject.BLL.Services
{
    public class TopUpService : ITopUpService
    {
        private const int PaymentExpiryMinutes = 15;

        private readonly IUnitOfWork _uow;
        private readonly FitupPayOSOptions _opt;
        private readonly PayOSClient _payOS;

        public TopUpService(IUnitOfWork uow, IOptions<FitupPayOSOptions> opt)
        {
            _uow = uow;
            _opt = opt.Value;

            ValidateConfiguration(_opt);
            _payOS = new PayOSClient(_opt.ClientId, _opt.ApiKey, _opt.ChecksumKey);
        }

        public async Task<CreateTopUpResultDto> CreateTopUpAsync(string accountId, CreateTopUpDto dto)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new Exception("AccountId is required");

            if (dto is null)
                throw new Exception("Top-up payload is required");

            if (string.IsNullOrWhiteSpace(dto.ConversionRateId))
                throw new Exception("ConversionRateId is required");

            ValidateAmount(dto.AmountVnd);

            var accRepo = _uow.GetRepository<Account>();
            var rateRepo = _uow.GetRepository<ConversionRate>();
            var payRepo = _uow.GetRepository<Payment>();

            var accountExists = await accRepo.Entities.AnyAsync(x => x.Id == accountId);
            if (!accountExists)
                throw new Exception("Account not found");

            var rate = await rateRepo.Entities
                .FirstOrDefaultAsync(x => x.Id == dto.ConversionRateId);

            if (rate == null || rate.Status != ConversionRateStatus.Active)
                throw new Exception("ConversionRate not found or inactive");

            var now = DateTimeOffset.UtcNow;
            var expiredAt = now.AddMinutes(PaymentExpiryMinutes);
            var orderCode = await GenerateUniqueOrderCodeAsync(payRepo);

            var payment = new Payment
            {
                AccountId = accountId,
                ConversionRateId = dto.ConversionRateId,
                Amount = dto.AmountVnd,
                Status = PaymentStatus.Pending,
                Method = PaymentMethod.PayOS,
                OrderCode = orderCode,
                ExpiredAt = expiredAt,
                CreatedAt = now,
                CreatedBy = accountId,
                UpdatedAt = now,
                UpdatedBy = accountId
            };

            await payRepo.AddAsync(payment);
            await _uow.SaveAsync();

            try
            {
                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = decimal.ToInt32(dto.AmountVnd),
                    Description = $"TOPUP {payment.Id[..8]}",
                    CancelUrl = _opt.CancelUrl,
                    ReturnUrl = _opt.ReturnUrl
                };

                var paymentLink = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

                if (string.IsNullOrWhiteSpace(paymentLink.CheckoutUrl))
                    throw new Exception("PayOS did not return a checkout URL");

                payment.CheckoutUrl = paymentLink.CheckoutUrl;
                payment.UpdatedAt = DateTimeOffset.UtcNow;
                payment.UpdatedBy = accountId;

                payRepo.Update(payment);
                await _uow.SaveAsync();

                return new CreateTopUpResultDto(
                    payment.Id,
                    payment.Amount,
                    orderCode,
                    paymentLink.CheckoutUrl,
                    payment.ExpiredAt
                );
            }
            catch (Exception ex)
            {
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTimeOffset.UtcNow;
                payment.UpdatedBy = accountId;

                payRepo.Update(payment);
                await _uow.SaveAsync();

                throw new Exception($"Unable to create PayOS payment link: {ex.Message}");
            }
        }

        public async Task<IEnumerable<PaymentListItemDto>> GetMyTopUpsAsync(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new Exception("AccountId is required");

            var payRepo = _uow.GetRepository<Payment>();

            return await payRepo.Entities
                .Where(x => x.AccountId == accountId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new PaymentListItemDto(
                    x.Id,
                    x.AccountId,
                    x.Amount,
                    x.Status,
                    x.Method,
                    x.OrderCode,
                    x.CheckoutUrl,
                    x.PaidAt,
                    x.ExpiredAt,
                    x.ConfirmedAt,
                    x.CreatedAt
                ))
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentListItemDto>> GetAllTopUpsAsync()
        {
            var payRepo = _uow.GetRepository<Payment>();

            return await payRepo.Entities
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new PaymentListItemDto(
                    x.Id,
                    x.AccountId,
                    x.Amount,
                    x.Status,
                    x.Method,
                    x.OrderCode,
                    x.CheckoutUrl,
                    x.PaidAt,
                    x.ExpiredAt,
                    x.ConfirmedAt,
                    x.CreatedAt
                ))
                .ToListAsync();
        }

        public async Task<PaymentStatusDto> GetPaymentStatusAsync(string paymentId, string accountId)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
                throw new Exception("PaymentId is required");

            if (string.IsNullOrWhiteSpace(accountId))
                throw new Exception("AccountId is required");

            var payRepo = _uow.GetRepository<Payment>();

            var payment = await payRepo.Entities
                .FirstOrDefaultAsync(x => x.Id == paymentId && x.AccountId == accountId);

            if (payment == null)
                throw new Exception("Payment not found");

            if (payment.Status == PaymentStatus.Pending
                && payment.ExpiredAt.HasValue
                && payment.ExpiredAt.Value < DateTimeOffset.UtcNow)
            {
                MarkPaymentCancelled(payment, accountId);
                payRepo.Update(payment);
                await _uow.SaveAsync();
            }

            return new PaymentStatusDto(
                payment.Id,
                payment.Amount,
                payment.Status,
                payment.OrderCode,
                payment.CheckoutUrl,
                payment.PaidAt,
                payment.ExpiredAt,
                payment.ConfirmedAt
            );
        }

        public async Task CancelExpiredPendingPaymentAsync(string paymentId, string accountId)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
                throw new Exception("PaymentId is required");

            if (string.IsNullOrWhiteSpace(accountId))
                throw new Exception("AccountId is required");

            var payRepo = _uow.GetRepository<Payment>();

            var payment = await payRepo.Entities
                .FirstOrDefaultAsync(x => x.Id == paymentId && x.AccountId == accountId);

            if (payment == null)
                throw new Exception("Payment not found");

            if (payment.Status != PaymentStatus.Pending)
                throw new Exception("Only pending payment can be cancelled");

            if (payment.ExpiredAt.HasValue && payment.ExpiredAt.Value > DateTimeOffset.UtcNow)
                throw new Exception("Payment has not expired yet");

            MarkPaymentCancelled(payment, accountId);
            payRepo.Update(payment);
            await _uow.SaveAsync();
        }

        public async Task HandleWebhookAsync(Webhook? webhook)
        {
            if (webhook?.Data == null)
                throw new Exception("Webhook payload is required");

            WebhookData verifiedData;
            try
            {
                verifiedData = await _payOS.Webhooks.VerifyAsync(webhook);
            }
            catch (Exception ex)
            {
                throw new Exception($"Invalid payOS webhook: {ex.Message}");
            }

            var payRepo = _uow.GetRepository<Payment>();
            var accRepo = _uow.GetRepository<Account>();
            var rateRepo = _uow.GetRepository<ConversionRate>();

            var payment = await payRepo.Entities
                .FirstOrDefaultAsync(x => x.OrderCode == verifiedData.OrderCode);

            if (payment == null)
                throw new Exception("Payment not found");

            if (payment.Status != PaymentStatus.Pending)
                return;

            if (!string.IsNullOrWhiteSpace(payment.ProviderTransactionId))
                return;

            if (payment.Amount != verifiedData.Amount)
                throw new Exception("Webhook amount does not match payment amount");

            var now = DateTimeOffset.UtcNow;

            if (!IsSuccessfulWebhook(webhook, verifiedData))
            {
                payment.Status = PaymentStatus.Failed;
                payment.ConfirmedAt = now;
                payment.ConfirmedBy = "PAYOS";
                payment.ProviderTransactionId = ResolveProviderTransactionId(verifiedData);
                payment.UpdatedAt = now;
                payment.UpdatedBy = "PAYOS";

                payRepo.Update(payment);
                await _uow.SaveAsync();
                return;
            }

            var rate = await rateRepo.Entities
                .FirstOrDefaultAsync(x => x.Id == payment.ConversionRateId);

            if (rate == null || rate.Status != ConversionRateStatus.Active)
                throw new Exception("ConversionRate not found or inactive");

            var account = await accRepo.Entities
                .FirstOrDefaultAsync(x => x.Id == payment.AccountId);

            if (account == null)
                throw new Exception("Account not found");

            var addPoint = decimal.Round(payment.Amount * rate.Rate, 2);

            payment.Status = PaymentStatus.Success;
            payment.PaidAt = now;
            payment.ConfirmedAt = now;
            payment.ConfirmedBy = "PAYOS";
            payment.ProviderTransactionId = ResolveProviderTransactionId(verifiedData);
            payment.UpdatedAt = now;
            payment.UpdatedBy = "PAYOS";

            account.PointAmount += addPoint;
            account.UpdatedAt = now;
            account.UpdatedBy = "PAYOS";

            payRepo.Update(payment);
            accRepo.Update(account);

            await _uow.SaveAsync();
        }

        private static void ValidateConfiguration(FitupPayOSOptions opt)
        {
            if (string.IsNullOrWhiteSpace(opt.ClientId))
                throw new Exception("Missing config: PayOS:ClientId");

            if (string.IsNullOrWhiteSpace(opt.ApiKey))
                throw new Exception("Missing config: PayOS:ApiKey");

            if (string.IsNullOrWhiteSpace(opt.ChecksumKey))
                throw new Exception("Missing config: PayOS:ChecksumKey");

            if (string.IsNullOrWhiteSpace(opt.ReturnUrl))
                throw new Exception("Missing config: PayOS:ReturnUrl");

            if (string.IsNullOrWhiteSpace(opt.CancelUrl))
                throw new Exception("Missing config: PayOS:CancelUrl");
        }

        private static void ValidateAmount(decimal amountVnd)
        {
            if (amountVnd <= 0)
                throw new Exception("Amount must be greater than 0");

            if (decimal.Truncate(amountVnd) != amountVnd)
                throw new Exception("Amount must be a whole number in VND");

            if (amountVnd > int.MaxValue)
                throw new Exception("Amount exceeds the supported limit");
        }

        private static void MarkPaymentCancelled(Payment payment, string updatedBy)
        {
            payment.Status = PaymentStatus.Cancelled;
            payment.UpdatedAt = DateTimeOffset.UtcNow;
            payment.UpdatedBy = updatedBy;
        }

        private static bool IsSuccessfulWebhook(Webhook webhook, WebhookData verifiedData)
        {
            return webhook.Success
                && string.Equals(webhook.Code, "00", StringComparison.OrdinalIgnoreCase)
                && string.Equals(verifiedData.Code, "00", StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveProviderTransactionId(WebhookData verifiedData)
        {
            return string.IsNullOrWhiteSpace(verifiedData.PaymentLinkId)
                ? verifiedData.OrderCode.ToString()
                : verifiedData.PaymentLinkId;
        }

        private static long GenerateOrderCode()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000L
                + Random.Shared.Next(0, 1000);
        }

        private static async Task<long> GenerateUniqueOrderCodeAsync(IGenericRepository<Payment> payRepo)
        {
            long orderCode;

            do
            {
                orderCode = GenerateOrderCode();
            }
            while (await payRepo.Entities.AnyAsync(x => x.OrderCode == orderCode));

            return orderCode;
        }

        public async Task HandleReturnAsync(long orderCode, string? code, string? status, bool? cancel)
        {
            if (orderCode <= 0)
                throw new Exception("OrderCode is required");

            var payRepo = _uow.GetRepository<Payment>();
            var accRepo = _uow.GetRepository<Account>();
            var rateRepo = _uow.GetRepository<ConversionRate>();

            var payment = await payRepo.Entities
                .FirstOrDefaultAsync(x => x.OrderCode == orderCode);

            if (payment == null)
                throw new Exception("Payment not found");

            // đã xử lý trước đó thì thôi
            if (payment.Status != PaymentStatus.Pending)
                return;

            if (!string.IsNullOrWhiteSpace(payment.ProviderTransactionId))
                return;

            var now = DateTimeOffset.UtcNow;

            // user hủy hoặc return không thành công
            if (cancel == true
                || !string.Equals(code, "00", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase))
            {
                payment.Status = PaymentStatus.Failed;
                payment.ConfirmedAt = now;
                payment.ConfirmedBy = "PAYOS_RETURN";
                payment.ProviderTransactionId = $"RETURN-{orderCode}";
                payment.UpdatedAt = now;
                payment.UpdatedBy = "PAYOS_RETURN";

                payRepo.Update(payment);
                await _uow.SaveAsync();
                return;
            }

            var rate = await rateRepo.Entities
                .FirstOrDefaultAsync(x => x.Id == payment.ConversionRateId);

            if (rate == null || rate.Status != ConversionRateStatus.Active)
                throw new Exception("ConversionRate not found or inactive");

            var account = await accRepo.Entities
                .FirstOrDefaultAsync(x => x.Id == payment.AccountId);

            if (account == null)
                throw new Exception("Account not found");

            var addPoint = decimal.Round(payment.Amount * rate.Rate, 2);

            payment.Status = PaymentStatus.Success;
            payment.PaidAt = now;
            payment.ConfirmedAt = now;
            payment.ConfirmedBy = "PAYOS_RETURN";
            payment.ProviderTransactionId = $"RETURN-{orderCode}";
            payment.UpdatedAt = now;
            payment.UpdatedBy = "PAYOS_RETURN";

            account.PointAmount += addPoint;
            account.UpdatedAt = now;
            account.UpdatedBy = "PAYOS_RETURN";

            payRepo.Update(payment);
            accRepo.Update(account);

            await _uow.SaveAsync();
        }
    }
}
