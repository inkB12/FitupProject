using FitupProject.BLL.DTOs.Premium;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;

namespace FitupProject.BLL.Services
{
    public class PremiumService : IPremiumService
    {
        private readonly IUnitOfWork _uow;

        public PremiumService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PurchasePremiumResponse> PurchasePremiumAsync(string accountId, PurchasePremiumRequest request)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new Exception("AccountId is required.");

            if (string.IsNullOrWhiteSpace(request.PremiumTypeId))
                throw new Exception("PremiumTypeId is required.");

            var accountRepo = _uow.GetRepository<Account>();
            var premiumTypeRepo = _uow.GetRepository<PremiumType>();
            var premiumRepo = _uow.GetRepository<Premium>();
            var servicePaymentRepo = _uow.GetRepository<ServicePayment>();
            var premiumPaymentRepo = _uow.GetRepository<PremiumPayment>();

            var account = (await accountRepo.FindAsync(x => x.Id == accountId)).FirstOrDefault();
            if (account == null)
                throw new Exception("Account not found.");

            var premiumType = (await premiumTypeRepo.FindAsync(x =>
                x.Id == request.PremiumTypeId &&
                x.Status == PremiumTypeStatus.Active)).FirstOrDefault();

            if (premiumType == null)
                throw new Exception("PremiumType not found or inactive.");

            if (account.PointAmount < premiumType.Price)
                throw new Exception("Not enough points to purchase premium.");

            var now = DateTimeOffset.UtcNow;

            var lastPremium = (await premiumRepo.FindAsync(
                x => x.AccountId == accountId,
                q => q.OrderByDescending(x => x.EndDate)))
                .FirstOrDefault();

            DateTimeOffset startDate;

            if (lastPremium != null && lastPremium.EndDate > now)
            {
                startDate = lastPremium.EndDate;
            }
            else
            {
                startDate = now;
            }

            var endDate = startDate.AddDays(premiumType.Duration);

            account.PointAmount -= premiumType.Price;
            account.UpdatedAt = now;
            account.UpdatedBy = accountId;
            await accountRepo.UpdateAsync(account);

            var servicePayment = new ServicePayment
            {
                Amount = premiumType.Price,
                ServiceType = ServiceType.Premium,
                PaymentDate = now,
                Status = PaymentStatus.Success,
                CreatedAt = now,
                CreatedBy = accountId
            };
            await servicePaymentRepo.AddAsync(servicePayment);

            var premium = new Premium
            {
                PremiumTypeId = premiumType.Id,
                AccountId = accountId,
                StartDate = startDate,
                EndDate = endDate,
                Status = PremiumStatus.Active,
                CreatedAt = now,
                CreatedBy = accountId
            };
            await premiumRepo.AddAsync(premium);

            var premiumPayment = new PremiumPayment
            {
                PremiumId = premium.Id,
                ServicePaymentId = servicePayment.Id,
                Price = premiumType.Price,
                CreatedAt = now,
                CreatedBy = accountId
            };
            await premiumPaymentRepo.AddAsync(premiumPayment);

            await _uow.SaveAsync();

            return new PurchasePremiumResponse
            {
                PremiumId = premium.Id,
                PremiumTypeId = premiumType.Id,
                DurationDays = premiumType.Duration,
                Price = premiumType.Price,
                StartDate = startDate,
                EndDate = endDate,
                RemainingPointAmount = account.PointAmount,
                ServicePaymentId = servicePayment.Id,
                Message = "Purchase premium successfully."
            };
        }

        public async Task<MyPremiumStatusResponse> GetMyPremiumStatusAsync(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new Exception("AccountId is required.");

            var premiumRepo = _uow.GetRepository<Premium>();
            var now = DateTimeOffset.UtcNow;

            // Chỉ lấy premium còn status Active
            // Premium bị Cancelled sẽ tự động biến mất khỏi kết quả
            var premiums = (await premiumRepo.FindAsync(
                x => x.AccountId == accountId && x.Status == PremiumStatus.Active,
                q => q.OrderBy(x => x.StartDate)))
                .ToList();

            if (!premiums.Any())
            {
                return new MyPremiumStatusResponse
                {
                    HasPremium = false,
                    IsActive = false,
                    RemainingDays = 0
                };
            }

            // Tìm premium đang cover thời điểm hiện tại
            var current = premiums.FirstOrDefault(x => x.StartDate <= now && x.EndDate > now);

            if (current == null)
            {
                return new MyPremiumStatusResponse
                {
                    HasPremium = false,
                    IsActive = false,
                    RemainingDays = 0
                };
            }

            var currentIndex = premiums.IndexOf(current);
            var mergedStart = current.StartDate;
            var mergedEnd = current.EndDate;

            // Gộp ngược về trước
            for (int i = currentIndex - 1; i >= 0; i--)
            {
                var prev = premiums[i];

                // chỉ merge nếu liền nhau/chồng nhau
                if (prev.EndDate >= mergedStart)
                {
                    if (prev.StartDate < mergedStart)
                        mergedStart = prev.StartDate;

                    if (prev.EndDate > mergedEnd)
                        mergedEnd = prev.EndDate;
                }
                else
                {
                    break;
                }
            }

            // Gộp tới sau
            for (int i = currentIndex + 1; i < premiums.Count; i++)
            {
                var next = premiums[i];

                // chỉ merge nếu liền nhau/chồng nhau
                if (next.StartDate <= mergedEnd)
                {
                    if (next.EndDate > mergedEnd)
                        mergedEnd = next.EndDate;
                }
                else
                {
                    break;
                }
            }

            var remainingDays = mergedEnd > now
                ? (int)Math.Ceiling((mergedEnd - now).TotalDays)
                : 0;

            return new MyPremiumStatusResponse
            {
                HasPremium = true,
                IsActive = true,
                PremiumId = current.Id,
                PremiumTypeId = current.PremiumTypeId,
                StartDate = mergedStart,
                EndDate = mergedEnd,
                RemainingDays = remainingDays
            };
        }

        public async Task<IEnumerable<PremiumTypeResponse>> GetActivePremiumTypesAsync()
        {
            var premiumTypeRepo = _uow.GetRepository<PremiumType>();

            var items = await premiumTypeRepo.FindAsync(
                x => x.Status == PremiumTypeStatus.Active,
                q => q.OrderBy(x => x.Duration));

            return items.Select(x => new PremiumTypeResponse
            {
                Id = x.Id,
                Describe = x.Describe,
                Duration = x.Duration,
                Price = x.Price,
                Status = x.Status
            });
        }
    }
}
