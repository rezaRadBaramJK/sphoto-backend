using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Types;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services
{
    public class CompletedReservationServiceResults
    {
        public Customer Customer { get; set; }
        public int? AddressId { get; set; }
        
        public Product Product { get; set; }
        
        public Reservation Reservation { get; set; }
        
        public TechnicianReservationResultType Result { get; set; }
        
        public string ImageIds { get; set; }
        
        public string Note { get; set; }
    }
}