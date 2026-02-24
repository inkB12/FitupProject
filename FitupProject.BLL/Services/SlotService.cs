using FitupProject.BLL.DTOs.Slots;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.BLL.Services
{
    public class SlotService : ISlotService
    {
        private readonly IUnitOfWork _uow;

        public SlotService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<string> CreateSlotAsync(CreateSlotRequest request)
        {
            var ptRepo = _uow.GetRepository<PT>();
            var pt = await ptRepo.Entities.FirstOrDefaultAsync(p => p.AccountId == request.PTId);

            if (pt == null)
            {
                throw new Exception("Tài khoản của bạn chưa được đăng ký làm PT hoặc không tồn tại.");
            }

            var actualPTId = pt.Id;

            var slotRepo = _uow.GetRepository<Slot>();

            var existingSlots = await slotRepo.Entities
                .Where(s => s.PTId == actualPTId && s.DateInWeek == request.DateInWeek && s.Status == SlotStatus.Active)
                .ToListAsync();

            bool isOverlap = existingSlots.Any(s =>
                (request.SlotStart >= s.SlotStart && request.SlotStart < s.SlotEnd) ||
                (request.SlotEnd > s.SlotStart && request.SlotEnd <= s.SlotEnd) ||
                (request.SlotStart <= s.SlotStart && request.SlotEnd >= s.SlotEnd)
            );

            if (isOverlap)
                throw new Exception("Khung giờ này đã bị trùng với một lịch khác trong cùng thứ.");

            var slot = new Slot
            {
                PTId = actualPTId,
                SlotStart = request.SlotStart,
                SlotEnd = request.SlotEnd,
                DateInWeek = request.DateInWeek,
                Price = request.Price,
                Status = SlotStatus.Active
            };

            await slotRepo.AddAsync(slot);

            var sfbRepo = _uow.GetRepository<SlotForBooking>();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            int daysUntilNextDay = ((int)slot.DateInWeek - (int)today.DayOfWeek + 7) % 7;
            var firstDate = today.AddDays(daysUntilNextDay);

            for (int i = 0; i < 4; i++)
            {
                var bookingDate = firstDate.AddDays(i * 7);
                var sfb = new SlotForBooking
                {
                    Slot = slot,
                    BookingDate = bookingDate,
                    Price = slot.Price,
                    Status = SlotForBookingStatus.Available
                };
                await sfbRepo.AddAsync(sfb);
            }

            await _uow.SaveAsync();

            return slot.Id;
        }

        public async Task<IEnumerable<SlotResponse>> GetSlotsAsync(string ptId)
        {
            var ptRepo = _uow.GetRepository<PT>();
            var pt = await ptRepo.Entities.FirstOrDefaultAsync(p => p.AccountId == ptId);
            var actualPTId = pt?.Id ?? ptId;

            var slotRepo = _uow.GetRepository<Slot>();
            return await slotRepo.Entities
                .Where(s => s.PTId == actualPTId && s.Status == SlotStatus.Active)
                .Select(s => new SlotResponse
                {
                    Id = s.Id,
                    PTId = s.PTId,
                    SlotStart = s.SlotStart,
                    SlotEnd = s.SlotEnd,
                    DateInWeek = s.DateInWeek,
                    Price = s.Price,
                    Status = s.Status.ToString()
                })
                .ToListAsync();
        }

        public async Task DeleteSlotAsync(string slotId, string ptId)
        {
            var ptRepo = _uow.GetRepository<PT>();
            var pt = await ptRepo.Entities.FirstOrDefaultAsync(p => p.AccountId == ptId);
            var actualPTId = pt?.Id ?? ptId;

            var slotRepo = _uow.GetRepository<Slot>();
            var slot = await slotRepo.Entities.FirstOrDefaultAsync(s => s.Id == slotId && s.PTId == actualPTId);

            if (slot == null) throw new Exception("Không tìm thấy slot.");

            slot.Status = SlotStatus.Inactive;

            var sfbRepo = _uow.GetRepository<SlotForBooking>();
            var futureSfbs = await sfbRepo.Entities
                .Where(x => x.SlotId == slotId && x.BookingDate >= DateOnly.FromDateTime(DateTime.UtcNow) && x.Status == SlotForBookingStatus.Available)
                .ToListAsync();

            foreach (var sfb in futureSfbs)
            {
                sfb.Status = SlotForBookingStatus.Cancelled;
            }

            await _uow.SaveAsync();
        }

        public async Task<IEnumerable<SlotForBookingResponse>> GetWeeklyCalendarAsync(string ptId, DateOnly startDate)
        {
            var ptRepo = _uow.GetRepository<PT>();
            var pt = await ptRepo.Entities.FirstOrDefaultAsync(p => p.AccountId == ptId);
            var actualPTId = pt?.Id ?? ptId;

            var endDate = startDate.AddDays(7);
            var sfbRepo = _uow.GetRepository<SlotForBooking>();

            return await sfbRepo.Entities
                .Include(x => x.Slot)
                .Where(x => x.Slot!.PTId == actualPTId && x.BookingDate >= startDate && x.BookingDate < endDate)
                .OrderBy(x => x.BookingDate).ThenBy(x => x.Slot!.SlotStart)
                .Select(x => new SlotForBookingResponse
                {
                    Id = x.Id,
                    BookingDate = x.BookingDate,
                    StartTime = x.Slot!.SlotStart,
                    EndTime = x.Slot!.SlotEnd,
                    Price = x.Price,
                    Status = x.Status.ToString()
                })
                .ToListAsync();
        }

        public async Task CancelSlotForBookingAsync(string slotForBookingId, string ptId)
        {
            var ptRepo = _uow.GetRepository<PT>();
            var pt = await ptRepo.Entities.FirstOrDefaultAsync(p => p.AccountId == ptId);
            var actualPTId = pt?.Id ?? ptId;

            var sfbRepo = _uow.GetRepository<SlotForBooking>();
            var sfb = await sfbRepo.Entities
                .Include(x => x.Slot)
                .FirstOrDefaultAsync(x => x.Id == slotForBookingId && x.Slot!.PTId == actualPTId);

            if (sfb == null) throw new Exception("Không tìm thấy lịch hẹn.");

            if (sfb.Status == SlotForBookingStatus.Booked)
            {
                throw new Exception("Lịch này đã được đặt, không thể hủy trực tiếp. Vui lòng liên hệ bộ phận hỗ trợ.");
            }

            sfb.Status = SlotForBookingStatus.Cancelled;
            await _uow.SaveAsync();
        }

        public async Task UpdateSlotAsync(string slotId, UpdateSlotRequest request)
        {
            var ptRepo = _uow.GetRepository<PT>();
            var pt = await ptRepo.Entities.FirstOrDefaultAsync(p => p.AccountId == request.PTId);

            if (pt == null)
            {
                throw new Exception("Tài khoản của bạn chưa được đăng ký làm PT hoặc không tồn tại.");
            }

            var actualPTId = pt.Id;

            var slotRepo = _uow.GetRepository<Slot>();
            var slot = await slotRepo.Entities.FirstOrDefaultAsync(s => s.Id == slotId && s.PTId == actualPTId);

            if (slot == null)
            {
                throw new Exception("Không tìm thấy slot hoặc bạn không có quyền chỉnh sửa slot này.");
            }

            // Kiểm tra trùng lặp giờ nếu có thay đổi thời gian hoặc thứ
            if (slot.SlotStart != request.SlotStart || slot.SlotEnd != request.SlotEnd || slot.DateInWeek != request.DateInWeek)
            {
                var existingSlots = await slotRepo.Entities
                    .Where(s => s.PTId == actualPTId && s.DateInWeek == request.DateInWeek && s.Status == SlotStatus.Active && s.Id != slotId)
                    .ToListAsync();

                bool isOverlap = existingSlots.Any(s =>
                    (request.SlotStart >= s.SlotStart && request.SlotStart < s.SlotEnd) ||
                    (request.SlotEnd > s.SlotStart && request.SlotEnd <= s.SlotEnd) ||
                    (request.SlotStart <= s.SlotStart && request.SlotEnd >= s.SlotEnd)
                );

                if (isOverlap)
                    throw new Exception("Khung giờ cập nhật bị trùng với một lịch khác của bạn.");
            }

            // Cập nhật thông tin slot mẫu
            slot.SlotStart = request.SlotStart;
            slot.SlotEnd = request.SlotEnd;
            slot.DateInWeek = request.DateInWeek;
            slot.Price = request.Price;

            // Cập nhật các SlotForBooking trong tương lai chưa được đặt (Status == Available)
            var sfbRepo = _uow.GetRepository<SlotForBooking>();
            var futureSfbs = await sfbRepo.Entities
                .Where(x => x.SlotId == slotId && x.BookingDate >= DateOnly.FromDateTime(DateTime.UtcNow) && x.Status == SlotForBookingStatus.Available)
                .ToListAsync();

            foreach (var sfb in futureSfbs)
            {
                // Nếu đổi thứ, tính lại BookingDate
                if (slot.DateInWeek != request.DateInWeek)
                {
                    int daysDiff = (int)request.DateInWeek - (int)sfb.BookingDate.DayOfWeek;
                    sfb.BookingDate = sfb.BookingDate.AddDays(daysDiff);
                }
                sfb.Price = request.Price;
            }

            await _uow.SaveAsync();
        }
    }
}