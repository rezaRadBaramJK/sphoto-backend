using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking
{
    public interface ITimeSlotService
    {
        Task<List<TimeSlotModel>> GetTimeSlotModelListAsync(string typeName = null, int? productId = null,
            DayOfWeek? dayOfWeek = null);

        Task<bool> ItIsBookingItemAsync(int productId);
        Task<TimeSlot> AddAsync(AddTimeSlotModel model);
        Task<TimeSlot> GetByIdAsync(int id);
        Task<TimeSlot> GetByProductAttributeValueIdAsync(int productAttributeValueId);
        Task<TimeSlot> EditAsync(AddTimeSlotModel model);
        Task<TimeSlot> DeleteAsync(int id);
        Task<List<int>> GetProductIdsAsync(string typeName = null);
        Task<List<int>> GetDependTimeSlotIdsAsync(int timeSlotId);
        Task<List<TimeSlot>> GetDependTimeSlotAsync(int timeSlotId);

        Task<bool> CheckConflictAsync(int productId, DayOfWeek dayOfWeek, string typeName, DateTime startTime, DateTime endTime, int? skipTimeSlotId = null);

        Task<TimeSlotAvailabilityServiceResults> IsAvailableAsync(int timeSlotId, DateTime date);
        Task<TimeSlotAvailabilityServiceResults> IsAvailableAsync(TimeSlot timeSlotToCheck, DateTime date);
    }
}