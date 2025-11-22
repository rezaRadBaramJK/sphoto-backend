using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Domains;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services
{
    public class ReservationResultsServiceResults
    {
        public Reservation Reservation { get; set; }
        
        public Order Order { get; set; }
        
        public Customer Customer { get; set; }
        
        public TechnicianRelReservation TechnicianReservation { get; set; }
        
        
    }
}