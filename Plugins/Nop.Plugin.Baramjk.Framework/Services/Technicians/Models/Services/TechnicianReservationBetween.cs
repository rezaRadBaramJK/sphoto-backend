using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Domains;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services
{
    public class TechnicianReservationBetween
    {
        public Reservation Reservation { get; set; }
        
        public Technician Technician { get; set; }
        
        public Product Product { get; set; }
        
        public Order Order { get; set; }
        
        public Customer Customer { get; set; }
        
        public Address Address { get; set; }
        
    }
}