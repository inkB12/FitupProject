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

            var sfb = await sfbRepo.Entities
                .Include(x => x.Slot)
                .FirstOrDefaultAsync(x => x.Id == request.SlotForBookingId);

            if (sfb == null) throw new Exception("Không tìm thấy khung giờ này.");
            if (sfb.Status != SlotForBookingStatus.Available)
                throw new Exception("Khung giờ này đã có người khác đặt.");

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                SlotForBookingId = sfb.Id,
                AccountId = userId,
                Note = request.Note,
                Total = sfb.Price,
                Status = BookingStatus.Pending 
            };

            sfb.Status = SlotForBookingStatus.Booked;

            await bookingRepo.AddAsync(booking);
            await _uow.SaveAsync();

            return booking.Id;
        }

        public async Task CancelBookingAsync(string bookingId, string accountId)
        {
            var bookingRepo = _uow.GetRepository<Booking>();
            var sfbRepo = _uow.GetRepository<SlotForBooking>();

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
            {
                sfb.Status = SlotForBookingStatus.Available;
            }

            await _uow.SaveAsync();
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

            await reviewRepo.AddAsync(review);
            await _uow.SaveAsync();
        }
    }
}
