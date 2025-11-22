using System;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Types;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking.Models
{
    public class ReservationModel : CamelCaseBaseDto
    {
        public int Id { get; set; }
        public int TimeSlotId { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public DateTime ReservationStart { get; set; }
        public DateTime ReservationEnd { get; set; }
        public ReservationStatus Status { get; set; }
        public string StatusTitle => Status.ToString();
        public string ProductName { get; set; }
        public string TypeName { get; set; }
        public string Color { get; set; }
        public bool IsPied { get; set; }
        public decimal Cost { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Date { get; set; }
    }
}