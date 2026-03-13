using FitupProject.BLL.DTOs.ServicePayment;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
using FitupProject.DAL.Interfaces;

namespace FitupProject.BLL.Services
{
    public class ServicePaymentService : IServicePaymentService
    {
        private readonly IUnitOfWork _uow;

        public ServicePaymentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<ServicePaymentHistoryResponse>> GetMyServicePaymentHistoryAsync(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new Exception("AccountId is required.");

            var servicePaymentRepo = _uow.GetRepository<Core.Entities.ServicePayment>();
            var premiumPaymentRepo = _uow.GetRepository<Core.Entities.PremiumPayment>();
            var premiumRepo = _uow.GetRepository<Core.Entities.Premium>();
            var bookingPaymentRepo = _uow.GetRepository<Core.Entities.BookingPayment>();
            var bookingRepo = _uow.GetRepository<Core.Entities.Booking>();

            var servicePayments = await servicePaymentRepo.FindAsync(
                x => true,
                q => q.OrderByDescending(x => x.PaymentDate));

            var premiumPayments = await premiumPaymentRepo.GetAllAsync();
            var premiums = await premiumRepo.GetAllAsync();
            var bookingPayments = await bookingPaymentRepo.GetAllAsync();
            var bookings = await bookingRepo.GetAllAsync();

            var premiumMap = (
                from pp in premiumPayments
                join p in premiums on pp.PremiumId equals p.Id
                where p.AccountId == accountId
                select new
                {
                    pp.ServicePaymentId,
                    PremiumId = p.Id,
                    AccountId = p.AccountId
                }
            ).ToList();

            var bookingMap = (
                from bp in bookingPayments
                join b in bookings on bp.BookingId equals b.Id
                where b.AccountId == accountId
                select new
                {
                    bp.ServicePaymentId,
                    BookingId = b.Id,
                    AccountId = b.AccountId
                }
            ).ToList();

            var result = servicePayments
                .Where(sp =>
                    premiumMap.Any(x => x.ServicePaymentId == sp.Id) ||
                    bookingMap.Any(x => x.ServicePaymentId == sp.Id))
                .Select(sp =>
                {
                    var premiumRef = premiumMap.FirstOrDefault(x => x.ServicePaymentId == sp.Id);
                    var bookingRef = bookingMap.FirstOrDefault(x => x.ServicePaymentId == sp.Id);

                    return new ServicePaymentHistoryResponse
                    {
                        ServicePaymentId = sp.Id,
                        Amount = sp.Amount,
                        ServiceType = sp.ServiceType,
                        PaymentDate = sp.PaymentDate,
                        Status = sp.Status,
                        PremiumId = premiumRef?.PremiumId,
                        BookingId = bookingRef?.BookingId,
                        AccountId = premiumRef?.AccountId ?? bookingRef?.AccountId
                    };
                });

            return result;
        }

        public async Task<IEnumerable<ServicePaymentHistoryResponse>> GetAllServicePaymentHistoryAsync()
        {
            var servicePaymentRepo = _uow.GetRepository<Core.Entities.ServicePayment>();
            var premiumPaymentRepo = _uow.GetRepository<Core.Entities.PremiumPayment>();
            var premiumRepo = _uow.GetRepository<Core.Entities.Premium>();
            var bookingPaymentRepo = _uow.GetRepository<Core.Entities.BookingPayment>();
            var bookingRepo = _uow.GetRepository<Core.Entities.Booking>();

            var servicePayments = await servicePaymentRepo.FindAsync(
                x => true,
                q => q.OrderByDescending(x => x.PaymentDate));

            var premiumPayments = await premiumPaymentRepo.GetAllAsync();
            var premiums = await premiumRepo.GetAllAsync();
            var bookingPayments = await bookingPaymentRepo.GetAllAsync();
            var bookings = await bookingRepo.GetAllAsync();

            var result = servicePayments.Select(sp =>
            {
                var premiumRef = (
                    from pp in premiumPayments
                    join p in premiums on pp.PremiumId equals p.Id
                    where pp.ServicePaymentId == sp.Id
                    select new
                    {
                        PremiumId = p.Id,
                        AccountId = p.AccountId
                    }
                ).FirstOrDefault();

                var bookingRef = (
                    from bp in bookingPayments
                    join b in bookings on bp.BookingId equals b.Id
                    where bp.ServicePaymentId == sp.Id
                    select new
                    {
                        BookingId = b.Id,
                        AccountId = b.AccountId
                    }
                ).FirstOrDefault();

                return new ServicePaymentHistoryResponse
                {
                    ServicePaymentId = sp.Id,
                    Amount = sp.Amount,
                    ServiceType = sp.ServiceType,
                    PaymentDate = sp.PaymentDate,
                    Status = sp.Status,
                    PremiumId = premiumRef?.PremiumId,
                    BookingId = bookingRef?.BookingId,
                    AccountId = premiumRef?.AccountId ?? bookingRef?.AccountId
                };
            });

            return result;
        }

        public async Task<ServicePaymentDetailResponse> GetServicePaymentDetailAsync(string servicePaymentId)
        {
            if (string.IsNullOrWhiteSpace(servicePaymentId))
                throw new Exception("ServicePaymentId is required.");

            var servicePaymentRepo = _uow.GetRepository<Core.Entities.ServicePayment>();
            var premiumPaymentRepo = _uow.GetRepository<Core.Entities.PremiumPayment>();
            var premiumRepo = _uow.GetRepository<Core.Entities.Premium>();
            var bookingPaymentRepo = _uow.GetRepository<Core.Entities.BookingPayment>();
            var bookingRepo = _uow.GetRepository<Core.Entities.Booking>();

            var servicePayment = (await servicePaymentRepo.FindAsync(x => x.Id == servicePaymentId))
                .FirstOrDefault();

            if (servicePayment == null)
                throw new Exception("ServicePayment not found.");

            var response = new ServicePaymentDetailResponse
            {
                ServicePaymentId = servicePayment.Id,
                Amount = servicePayment.Amount,
                ServiceType = servicePayment.ServiceType,
                PaymentDate = servicePayment.PaymentDate,
                Status = servicePayment.Status
            };

            if (servicePayment.ServiceType == ServiceType.Premium)
            {
                var premiumPayment = (await premiumPaymentRepo.FindAsync(x => x.ServicePaymentId == servicePaymentId))
                    .FirstOrDefault();

                if (premiumPayment != null)
                {
                    var premium = (await premiumRepo.FindAsync(x => x.Id == premiumPayment.PremiumId))
                        .FirstOrDefault();

                    if (premium != null)
                    {
                        response.PremiumPaymentDetail = new PremiumPaymentDetailDto
                        {
                            PremiumPaymentId = premiumPayment.Id,
                            Price = premiumPayment.Price,
                            PremiumId = premium.Id,
                            PremiumTypeId = premium.PremiumTypeId,
                            AccountId = premium.AccountId,
                            StartDate = premium.StartDate,
                            EndDate = premium.EndDate,
                            PremiumStatus = premium.Status
                        };
                    }
                }
            }
            else if (servicePayment.ServiceType == ServiceType.BookingPT)
            {
                var bookingPayment = (await bookingPaymentRepo.FindAsync(x => x.ServicePaymentId == servicePaymentId))
                    .FirstOrDefault();

                if (bookingPayment != null)
                {
                    var booking = (await bookingRepo.FindAsync(x => x.Id == bookingPayment.BookingId))
                        .FirstOrDefault();

                    if (booking != null)
                    {
                        response.BookingPaymentDetail = new BookingPaymentDetailDto
                        {
                            BookingPaymentId = bookingPayment.Id,
                            Price = bookingPayment.Price,
                            BookingId = booking.Id,
                            AccountId = booking.AccountId,
                            SlotForBookingId = booking.SlotForBookingId,
                            Total = booking.Total,
                            Note = booking.Note,
                            BookingStatus = booking.Status
                        };
                    }
                }
            }

            return response;
        }

        public async Task<ServicePaymentDetailResponse> GetMyServicePaymentDetailAsync(string accountId, string servicePaymentId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new Exception("AccountId is required.");

            var detail = await GetServicePaymentDetailAsync(servicePaymentId);

            if (detail.ServiceType == ServiceType.Premium)
            {
                if (detail.PremiumPaymentDetail == null || detail.PremiumPaymentDetail.AccountId != accountId)
                    throw new Exception("You do not have permission to view this service payment.");
            }
            else if (detail.ServiceType == ServiceType.BookingPT)
            {
                if (detail.BookingPaymentDetail == null || detail.BookingPaymentDetail.AccountId != accountId)
                    throw new Exception("You do not have permission to view this service payment.");
            }
            else
            {
                throw new Exception("Unsupported service type.");
            }

            return detail;
        }
    }
}
