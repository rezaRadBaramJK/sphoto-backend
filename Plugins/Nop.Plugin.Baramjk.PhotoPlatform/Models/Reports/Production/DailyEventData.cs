using System;
using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.Production
{
    public class DailyEventData
    {
        public DateTime EventDate { get; set; }

        public string EventName { get; set; }

        public List<TimeSlotData> TimeSlots { get; set; } = new();
    }
}