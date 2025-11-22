using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.ActorEventTimeSlots
{
    public class ActorEventTimeSlotsModel
    {
        public int Id { get; set; }
        public string ActorName { get; set; }

        public ActorPicture ActorPicture { get; set; }
        public bool Active { get; set; }
        public int TimeSlotId { get; set; }
        public int ActorEventId { get; set; }
    }
}