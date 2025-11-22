using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Abstractions
{
    public interface ITechnicianOrderService
    {
        Task<List<ReservationResultsServiceResults>> GetOrderReservationsAsync(int orderId);
        Task NotifyTechniciansOrderPaidAsync(Order order);

        /// <exception cref="NopException"></exception>
        Task CompleteOrderStatusAsync(int reservationId);
    }
}