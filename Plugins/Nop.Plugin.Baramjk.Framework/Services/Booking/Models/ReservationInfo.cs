using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Types;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking.Models
{
    public class ReservationInfo 
    {
        public int ReservationId { get; set; }
        public int TimeSlotId { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public string ReservationStart { get; set; }
        public string ReservationEnd { get; set; }
        public ReservationStatus Status { get; set; }
        public Customer Customer { get; set; }
        public string OrderState { get; set; }
        public string StatusTitle { get; set; }
        public string CustomerFullName { get; set; }
        public string CustomerEmail { get; set; }
        public string ProductName { get; set; }
        public string PaymentStatus { get; set; }
        public string TypeName { get; set; }
    }
}