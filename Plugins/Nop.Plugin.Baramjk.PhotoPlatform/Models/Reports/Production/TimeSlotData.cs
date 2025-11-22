using System;
using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.Production
{
    public class TimeSlotData
    {
        public int TimeSlotId { get; set; }

        public TimeSpan StartTime { get; set; }

        public List<ActorData> ActorsData { get; set; } = new();
    }
}