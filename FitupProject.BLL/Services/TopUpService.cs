using FitupProject.BLL.Commons.VNPay;
using FitupProject.BLL.DTOs.VNPay;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FitupProject.BLL.Services
{
    public class TopUpService : ITopUpService
    {
        private readonly IUnitOfWork _uow;
        private readonly VnPayOptions _opt;

        public TopUpService(IUnitOfWork uow, IOptions<VnPayOptions> opt)
        {
            _uow = uow;
            _opt = opt.Value;
        }

        public async Task<CreateTopUpResultDto> CreateTopUpAsync(string accountId, CreateTopUpDto dto, string ipAddress)
        {
            if (dto.AmountVnd <= 0) throw new Exception("Amount must be > 0");

            var rateRepo = _uow.GetRepository<ConversionRate>();
            var rate = await rateRepo.Entities.FirstOrDefaultAsync(x => x.Id == dto.ConversionRateId);
            if (rate == null || rate.Status != ConversionRateStatus.Active)
                throw new Exception("ConversionRate not found or inactive");

            var payRepo = _uow.GetRepository<Payment>();

            var payment = new Payment
            {
                AccountId = accountId,
                ConversionRateId = dto.ConversionRateId,
                Amount = dto.AmountVnd,
                Status = PaymentStatus.Pending,
                VnpTxnRef = Guid.NewGuid().ToString("N"), // unique per day ok (alphanumeric) :contentReference[oaicite:7]{index=7}
                ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(_opt.ExpireMinutes)
            };

            await payRepo.AddAsync(payment);
            await _uow.SaveAsync();

            var orderInfo = $"Nap point FitUp payment {payment.Id}";
            var payUrl = VnPayLibrary.CreatePaymentUrl(
                _opt,
                txnRef: payment.VnpTxnRef!,
                amountVnd: payment.Amount,
                orderInfo: orderInfo,
                ipAddress: ipAddress,
                nowUtc: DateTimeOffset.UtcNow);

            return new CreateTopUpResultDto(payment.Id, payUrl);
        }

        public async Task<(string rspCode, string message)> HandleIpnAsync(IQueryCollection query)
        {
            // 1) verify signature :contentReference[oaicite:8]{index=8}
            if (!VnPayLibrary.ValidateSignature(query, _opt.HashSecret))
                return ("97", "Invalid signature"); // :contentReference[oaicite:9]{index=9}

            // 2) parse basic fields
            var vnpTxnRef = query["vnp_TxnRef"].ToString();
            var vnpAmountStr = query["vnp_Amount"].ToString(); // VNPAY trả về *100 :contentReference[oaicite:10]{index=10}
            var vnpResponseCode = query["vnp_ResponseCode"].ToString();
            var vnpTransactionStatus = query["vnp_TransactionStatus"].ToString();

            if (!long.TryParse(vnpAmountStr, out var vnpAmount100))
                return ("99", "Invalid request");

            var amountVnd = vnpAmount100 / 100m;

            var payRepo = _uow.GetRepository<Payment>();
            var accRepo = _uow.GetRepository<Account>();

            var payment = await payRepo.Entities.FirstOrDefaultAsync(x => x.VnpTxnRef == vnpTxnRef);
            if (payment == null) return ("01", "Order not found"); // :contentReference[oaicite:11]{index=11}

            // idempotent
            if (payment.Status != PaymentStatus.Pending)
                return ("02", "Order already confirmed"); // :contentReference[oaicite:12]{index=12}

            if (payment.Amount != amountVnd)
                return ("04", "invalid amount"); // :contentReference[oaicite:13]{index=13}

            // 3) update payment status
            payment.VnpResponseCode = vnpResponseCode;
            payment.VnpTransactionStatus = vnpTransactionStatus;
            payment.VnpTransactionNo = query["vnp_TransactionNo"].ToString();
            payment.VnpBankCode = query["vnp_BankCode"].ToString();

            // “00” success theo docs (ResponseCode/TransactionStatus) :contentReference[oaicite:14]{index=14}
            var success = (vnpResponseCode == "00" && vnpTransactionStatus == "00");

            if (success)
            {
                payment.Status = PaymentStatus.Success;
                payment.PaidAt = DateTimeOffset.UtcNow;

                // cộng point
                var rateRepo = _uow.GetRepository<ConversionRate>();
                var rate = await rateRepo.Entities.FirstAsync(x => x.Id == payment.ConversionRateId);

                var account = await accRepo.Entities.FirstAsync(x => x.Id == payment.AccountId);

                // Quy ước: Rate = point / 1 VND (ví dụ 0.01 => 100k VND = 1000 point)
                var addPoint = decimal.Round(payment.Amount * rate.Rate, 2);
                account.PointAmount += addPoint;

                accRepo.Update(account);
            }
            else
            {
                if (vnpResponseCode == "24")
                    payment.Status = PaymentStatus.Cancelled;
                else
                    payment.Status = PaymentStatus.Failed;
            }

            payRepo.Update(payment);
            await _uow.SaveAsync();

            return ("00", "Confirm Success"); // :contentReference[oaicite:15]{index=15}
        }

        public async Task<object> HandleReturnAsync(IQueryCollection query)
        {
            // ReturnURL để FE gọi lấy status hiển thị
            var okSig = VnPayLibrary.ValidateSignature(query, _opt.HashSecret);
            var vnpTxnRef = query["vnp_TxnRef"].ToString();
            var vnpResponseCode = query["vnp_ResponseCode"].ToString();
            var vnpTransactionStatus = query["vnp_TransactionStatus"].ToString();

            var payRepo = _uow.GetRepository<Payment>();
            var payment = await payRepo.Entities.FirstOrDefaultAsync(x => x.VnpTxnRef == vnpTxnRef);

            return new
            {
                SignatureValid = okSig,
                PaymentId = payment?.Id,
                payment?.Status,
                vnpResponseCode,
                vnpTransactionStatus
            };
        }
    }
}
