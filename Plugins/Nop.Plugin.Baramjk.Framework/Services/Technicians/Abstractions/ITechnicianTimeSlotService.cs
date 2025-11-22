using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Abstractions
{
    public interface ITechnicianTimeSlotService
    {
        Task ClearByTimeSlotIdAsync(int timeSlotId);
        Task ClearAllTechnicianTimeSlotsAsync(int technicianId);

        /// <exception cref="NotFoundBusinessException"></exception>
        Task SubmitTimeSlotToTechnicianAsync(int technicianId, int timeSlotId);

        Task<int> GetSelectedTechnicianIdAsync(int timeSlotId);
        Task<int> GetVendorIdByTimeSlotIdAsync(int timeSlotId);
        Task<IList<GetVendorTimeSlotsResults>> GetTimeSlotsAsync(int vendorId, DateTime date);
        Task<bool> IsTimeSlotAvailableAsync(TimeSlot slot, DateTime dateTime);
        Task<int> CountTimeSlotReservationAsync(List<int> timeSlotIds, DateTime dateTime);
        Task<TimeSlotTechnicianResult[]> GetTechniciansByTimeSlotIdsAsync(int[] timeSlotIds);
        Task<bool> IsTimeSlotsBelongsToVendorAsync(int vendorId, IList<int> timeSlotIds);
        
        /// <exception cref="NopException"></exception>
        Task InsertAsync(IList<TechnicianTimeSlot> technicianTimeSlotsToAdd);

        Task DeleteAsync(TechnicianTimeSlot technicianTimeSlot);

        Task DeleteAsync(IList<TechnicianTimeSlot> technicianTimeSlots);

        Task<TechnicianTimeSlot> GetByTimeSlotIdAsync(int timeSlotId);

    }
}