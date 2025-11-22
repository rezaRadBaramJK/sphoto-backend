using System;
using Nop.Core;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking.Domains
{
    public class TimeSlot : BaseEntity
    {
        public int ProductId { get; set; }
        public int ProductAttributeId { get; set; }
        public int Quantity { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Cost { get; set; }
        public string Color { get; set; }
        public string TypeName { get; set; }
        
        public bool IsDepend { get; set; }
    }
}