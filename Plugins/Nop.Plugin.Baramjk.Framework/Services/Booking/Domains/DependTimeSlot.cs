using Nop.Core;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking.Domains
{
    public class DependTimeSlot: BaseEntity
    {
        public int ParentTimeSlotId { get; set; }
        
        public int DependTimeSlotId { get; set; } 
    }
}