using System;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking.Models
{
    public class TimeSlotModel 
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductAttributeId { get; set; }
        public int Quantity { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public double Cost { get; set; }
        public string Color { get; set; }
        public string TypeName { get; set; }
        public bool IsMorning { get; set; }
        
        public bool IsDepend { get; set; }
    }
}