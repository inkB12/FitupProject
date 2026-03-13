using FitupProject.BLL.DTOs.Booking;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitupProject.BLL.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _uow;

        public BookingService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<string> BookSlotAsync(CreateBookingRequest request, string userId)
        {
            var sfbRepo = _uow.GetRepository<SlotForBooking>();
            var bookingRepo = _uow.GetRepository<Booking>();
            var accountRepo = _uow.GetRepository<Account>();
            var servicePaymentRepo = _uow.GetRepository<ServicePayment>();
            var bookingPaymentRepo = _uow.GetRepository<BookingPayment>();

            var sfb = await sfbRepo.Entities
                .Include(x => x.Slot)
                .FirstOrDefaultAsync(x => x.Id == request.SlotForBookingId);

            if (sfb == null) throw new Exception("Không tìm thấy khung giờ này.");
            if (sfb.Status != SlotForBookingStatus.Available)
                throw new Exception("Khung giờ này đã có người khác đặt.");

            var account = await accountRepo.GetByIdAsync(userId);
            if (account == null) throw new Exception("Tài khoản không tồn tại.");

            if (account.PointAmount < sfb.Price)
            {
                throw new Exception($"Số dư không đủ. Bạn cần thêm {sfb.Price - account.PointAmount} điểm để đặt lịch.");
            }

            account.PointAmount -= sfb.Price;

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                SlotForBookingId = sfb.Id,
                AccountId = userId,
                Note = request.Note,
                Total = sfb.Price,
                Status = BookingStatus.Confirmed,
            };

            sfb.Status = SlotForBookingStatus.Booked;

            var servicePayment = new ServicePayment
            {
                Id = Guid.NewGuid().ToString(),
                Amount = sfb.Price,
                ServiceType = ServiceType.BookingPT,
                PaymentDate = DateTimeOffset.UtcNow,
                Status = PaymentStatus.Success,
            };

            var bookingPayment = new BookingPayment
            {
                Id = Guid.NewGuid().ToString(),
                BookingId = booking.Id,
                ServicePaymentId = servicePayment.Id,
                Price = sfb.Price,
            };

            await bookingRepo.AddAsync(booking);
            await servicePaymentRepo.AddAsync(servicePayment);
            await bookingPaymentRepo.AddAsync(bookingPayment);
            await _uow.SaveAsync();

            return booking.Id;
        }

        public async Task CancelBookingAsync(string bookingId, string accountId)
        {
            var bookingRepo = _uow.GetRepository<Booking>();
            var sfbRepo = _uow.GetRepository<SlotForBooking>();
            var accountRepo = _uow.GetRepository<Account>();
            var bookingPaymentRepo = _uow.GetRepository<BookingPayment>();
            var servicePaymentRepo = _uow.GetRepository<ServicePayment>();

            var booking = await bookingRepo.Entities
                .Include(b => b.SlotForBooking)
                    .ThenInclude(sfb => sfb.Slot)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.AccountId == accountId);

            if (booking == null)
                throw new Exception("Không tìm thấy thông tin đặt lịch hoặc bạn không có quyền hủy lịch này.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new Exception("Lịch tập này đã được hủy rồi.");

            var slotStartDateTime = booking.SlotForBooking!.BookingDate.ToDateTime(booking.SlotForBooking.Slot!.SlotStart);

            var now = DateTime.UtcNow.AddHours(7);

            if (now.AddHours(24) > slotStartDateTime)
            {
                throw new Exception("Bạn chỉ có thể hủy lịch tập trước khi bắt đầu ít nhất 24 giờ. Vui lòng liên hệ PT để được hỗ trợ.");
            }

            booking.Status = BookingStatus.Cancelled;

            var sfb = await sfbRepo.Entities.FirstOrDefaultAsync(x => x.Id == booking.SlotForBookingId);
            if (sfb != null)
                sfb.Status = SlotForBookingStatus.Available;

            var refundPoint = booking.Total;
            if (refundPoint > 0)
            {
                var account = await accountRepo.Entities
                    .FirstOrDefaultAsync(a => a.Id == booking.AccountId);

                if (account == null)
                    throw new KeyNotFoundException("Account không tồn tại.");

                account.PointAmount += refundPoint;
                await accountRepo.UpdateAsync(account);
            }

            var bookingPayment = await bookingPaymentRepo.Entities
                .FirstOrDefaultAsync(bp => bp.BookingId == bookingId);
            if (bookingPayment != null)
            {
                var servicePayment = await servicePaymentRepo.GetByIdAsync(bookingPayment.ServicePaymentId);
                if (servicePayment != null)
                    servicePayment.Status = PaymentStatus.Cancelled;
            }

            await _uow.SaveAsync();
        }

    
        public async Task<bool> ForceCancelBookingAsync(string bookingId)
        {
            var bookingRepo = _uow.GetRepository<Booking>();
            var accountRepo = _uow.GetRepository<Account>();
            var bookingPaymentRepo = _uow.GetRepository<BookingPayment>();
            var servicePaymentRepo = _uow.GetRepository<ServicePayment>();

            var booking = await bookingRepo.Entities
                .Include(b => b.Account)
                .Include(b => b.SlotForBooking)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                throw new KeyNotFoundException("Booking không tồn tại.");

            if (booking.Status != BookingStatus.Confirmed)
                throw new InvalidOperationException("Chỉ có thể Force Cancel booking ở trạng thái Confirmed.");

            var refundPoint = booking.Total;
            if (refundPoint > 0)
            {
                var account = await accountRepo.Entities
                    .FirstOrDefaultAsync(a => a.Id == booking.AccountId);

                if (account == null)
                    throw new KeyNotFoundException("Account không tồn tại.");

                account.PointAmount += refundPoint;
                await accountRepo.UpdateAsync(account);
            }

            booking.Status = BookingStatus.Cancelled;
            if (booking.SlotForBooking != null)
                booking.SlotForBooking.Status = SlotForBookingStatus.Available;

            var bookingPayment = await bookingPaymentRepo.Entities
                .FirstOrDefaultAsync(bp => bp.BookingId == bookingId);
            if (bookingPayment != null)
            {
                var servicePayment = await servicePaymentRepo.GetByIdAsync(bookingPayment.ServicePaymentId);
                if (servicePayment != null)
                    servicePayment.Status = PaymentStatus.Cancelled;
            }

            await _uow.SaveAsync();

            return true;
        }

        public async Task<IEnumerable<BookingResponse>> GetBookingsForAdminAsync(GetBookingPagingRequest request)
        {
            var bookingRepo = _uow.GetRepository<Booking>();
            var query = bookingRepo.Entities.AsQueryable();
            if(request.Status.HasValue)
            {
                query = query.Where(b => b.Status == request.Status.Value);
            }
            if (!string.IsNullOrEmpty(request.FromDate) && DateTimeOffset.TryParse(request.FromDate, out var from))
            {
                query = query.Where(b => b.CreatedAt >= from.ToUniversalTime());
            }

            if (!string.IsNullOrEmpty(request.ToDate) && DateTimeOffset.TryParse(request.ToDate, out var to))
            {
                query = query.Where(b => b.CreatedAt < to.ToUniversalTime());
            }
            var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var result = await query.Select(b => new BookingResponse
            {
                Id = b.Id,
                SlotForBookingId = b.SlotForBookingId,
                BookingDate = b.SlotForBooking!.BookingDate,
                StartTime = b.SlotForBooking.Slot!.SlotStart,
                EndTime = b.SlotForBooking.Slot.SlotEnd,
                Total = b.Total,
                Status = b.Status.ToString(),
                Note = b.Note,
                PTName = b.SlotForBooking.Slot.PT!.DisplayName
                }).ToListAsync();
            return result;
        }
        

        public async Task<IEnumerable<BookingResponse>> GetBookingsForUserAsync(string accountId)
        {
            var bookingRepo = _uow.GetRepository<Booking>();

            var bookings = await bookingRepo.Entities
                .Include(b => b.SlotForBooking)
                    .ThenInclude(sfb => sfb.Slot)
                        .ThenInclude(s => s.PT)
                .Where(b => b.AccountId == accountId)
                .Where(b => b.AccountId == accountId
                 && b.Status != BookingStatus.Cancelled)
                .OrderByDescending(b => b.SlotForBooking!.BookingDate) 
                .ToListAsync();

            // Map sang DTO
            return bookings.Select(b => new BookingResponse
            {
                Id = b.Id,
                SlotForBookingId = b.SlotForBookingId,
                BookingDate = b.SlotForBooking!.BookingDate,
                StartTime = b.SlotForBooking.Slot!.SlotStart,
                EndTime = b.SlotForBooking.Slot.SlotEnd,
                Total = b.Total,
                Status = b.Status.ToString(),
                Note = b.Note,
                PTName = b.SlotForBooking.Slot.PT?.DisplayName ?? "N/A"
            });
        }

        public async Task SendFeedbackAsync(SendFeedbackRequest request, string accountId)
        {
            var bookingRepo = _uow.GetRepository<Booking>();
            var reviewRepo = _uow.GetRepository<BookingReview>();

            var booking = await bookingRepo.Entities
                .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.AccountId == accountId);

            if (booking == null)
                throw new Exception("Không tìm thấy thông tin buổi tập.");

            if (booking.Status != BookingStatus.Completed)
            {
                throw new Exception("Bạn chỉ có thể đánh giá sau khi buổi tập đã kết thúc và được xác nhận hoàn thành.");
            }

            var isReviewed = await reviewRepo.Entities.AnyAsync(r => r.BookingId == request.BookingId);
            if (isReviewed)
                throw new Exception("Bạn đã gửi đánh giá cho buổi tập này rồi.");

            var review = new BookingReview
            {
                Id = Guid.NewGuid().ToString(),
                BookingId = request.BookingId,
                Rating = request.Rating,
                Comment = request.Comment
            };

            if (booking.SlotForBooking != null)
                booking.SlotForBooking.Status = SlotForBookingStatus.Available;

            await reviewRepo.AddAsync(review);
            await _uow.SaveAsync();
        }
    }
}
