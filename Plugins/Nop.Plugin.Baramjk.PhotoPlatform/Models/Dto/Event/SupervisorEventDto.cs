using System.Collections.Generic;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.TimeSlot;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Event
{
    public class SupervisorEventDto : EventDto
    {
        public List<GroupedTimeSlotsDto> OpenTimeSlots { get; set; }
        public List<GroupedTimeSlotsDto> ClosedTimeSlots { get; set; }
    }
}