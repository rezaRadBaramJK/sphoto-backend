using Nop.Core;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Domains
{
    public class TechnicianTimeSlot : BaseEntity
    {
        public int TechnicianId { get; set; }
        public int TimeSlotId { get; set; }
    }
}