using Nop.Core;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Domains
{
    public class RejectedReservation: BaseEntity
    {
        public int TechnicianId { get; set; }
        
        public int ReservationId { get; set; }
        
    }
}