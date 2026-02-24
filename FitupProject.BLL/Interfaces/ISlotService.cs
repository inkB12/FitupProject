using FitupProject.BLL.DTOs.Slots;

namespace FitupProject.BLL.Interfaces
{
    public interface ISlotService
    {
        Task<string> CreateSlotAsync(CreateSlotRequest request);
        
        Task<IEnumerable<SlotResponse>> GetSlotsAsync(string ptId);
        
        Task DeleteSlotAsync(string slotId, string ptId);

        Task<IEnumerable<SlotForBookingResponse>> GetWeeklyCalendarAsync(string ptId, DateOnly startDate);
        
        Task CancelSlotForBookingAsync(string slotForBookingId, string ptId);

        Task UpdateSlotAsync(string slotId, UpdateSlotRequest request);
    }
}
