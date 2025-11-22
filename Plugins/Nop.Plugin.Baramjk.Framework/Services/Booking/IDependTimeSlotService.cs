using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking
{
    public interface IDependTimeSlotService
    {
        Task<List<TimeSlot>> GetAvailableTimeSlotsAsync(
            int productId,
            int dayOfWeekId, 
            int timeSlotId,
            DateTime startTime,
            DateTime endTime);

        Task<List<DependTimeSlot>> GetDependTimeSlotsAsync(int timeSlotId);

        /// <exception cref="NopException"></exception>
        Task InsertAsync(int parentTimeSlotId, int dependTimeSlotId);
        
        /// <exception cref="NopException"></exception>
        Task InsertAsync(IList<DependTimeSlot> dependTimeSlots);

        Task DeleteAsync(DependTimeSlot dependTimeSlot);
        Task DeleteAsync(IList<DependTimeSlot> dependTimeSlots);
        
    }
}