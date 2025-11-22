using Nop.Core;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class ActorEventTimeSlot: BaseEntity
    {
        public int ActorEventId { get; set; }
        public int TimeSlotId { get; set; }
        public bool IsDeactivated { get; set; }
    }
}