using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking
{
    public interface IBookingTimeProductAttributeService
    {
        Task<AddSelectTimeAttributeResponse> AddSelectTimeAttributeAsync(
            int productId,
            decimal cost,
            DayOfWeek dayOfWeek,
            string startTime,
            string endTime,
            int quantity);

        
        /// <summary>
        /// create select time product attribute values and assign product attribute value id to associated time slot 
        /// </summary>
        /// <param name="timeSlots"></param>
        /// <param name="productId"></param>
        /// <exception cref="NotFoundBusinessException"></exception>
        Task<List<ProductAttributeValue>> AddSelectTimeAttributeValueForTimeSlotsAsync(IList<TimeSlot> timeSlots, int productId);

        Task UpdateSelectTimeAttributeAsync(int productAttributeValueId, AddTimeSlotModel timeSlotModel);
        Task<ProductAttributeValue> DeleteProductAttributeValueByIdAsync(int productAttributeValueId);
        Task<ProductAttributeMapping> DeleteSelectTimeAttributeAsync(int productId);

        Task DeleteAsync(IList<ProductAttributeValue> pavList);
    }
}