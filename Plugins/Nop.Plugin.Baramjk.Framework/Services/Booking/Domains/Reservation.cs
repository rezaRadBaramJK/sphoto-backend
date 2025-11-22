using System;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Types;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking.Domains
{
    public class Reservation : BaseEntity
    {
        public int TimeSlotId { get; set; }
        public string TimeSlotTypeName { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public DateTime ReservationStart { get; set; }
        public DateTime ReservationEnd { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime OnCreateUtc { get; set; }
        public decimal Cost { get; set; }
        public bool IsPied { get; set; }
    }
}