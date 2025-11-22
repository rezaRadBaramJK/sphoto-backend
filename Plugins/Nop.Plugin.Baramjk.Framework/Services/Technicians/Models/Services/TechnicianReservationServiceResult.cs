using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Types;


namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services
{
    public class TechnicianReservationServiceResult
    {
        public int? AddressId { get; set; }
        
        public Product Product { get; set; }
        
        public Reservation Reservation { get; set; }
        
        public bool IsApproved { get; set; }

        public TechnicianReservationProcessType Process { get; set; }
        
        public TechnicianReservationResultType Result { get; set; }
        

    }
}