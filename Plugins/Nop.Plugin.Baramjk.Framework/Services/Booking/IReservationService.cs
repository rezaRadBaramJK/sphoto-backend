using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Models;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Types;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking
{
    public interface IReservationService
    {
        Task<List<ReservationModel>> GetReservationAsync(string typeName = null, int? productId = null,
            ReservationStatus? status = null, DateTime? start = null, DateTime? end = null, int? customerId = null);

        Task<List<Reservation>> GetReservationAsync(int productId, DateTime dateTime, string typeName);
        Task<AddReservationResponse> AddAsync(TimeSlot slot, Reservation reservation, int itemQuantity);
        Task<Reservation> GetByIdAsync(int id);
        Task<ReservationInfo> GetInfo(int id);
        Task<Reservation> SetStatus(int id, ReservationStatus status);
        Task<bool> CheckCanReserveAsync(TimeSlot slot, DateTime dateTime, int itemQuantity);
        Task<Reservation> UpdateAsync(Reservation reservation);
        Task<Reservation> GetByOrderIdAsync(int orderId);

        Task<IPagedList<ReservationModel>> GetReservationAsync(ReservationResultType resultType, int pageIndex = 0,
            int pageSize = int.MaxValue);
        
        /// <exception cref="NopException"></exception>
        Task UpdateAsync(Reservation reservation, Order order, TimeSlot timeSlot = null, DateTime? dateTime = null);
    }
}