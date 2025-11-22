using System.Collections.Generic;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Reservation
{
    public class OrderReservationDetailsModel
    {
        public Order Order { get; set; }
        public List<ReservationDetailsModel> OrderReservationDetails { get; set; }
    }
}