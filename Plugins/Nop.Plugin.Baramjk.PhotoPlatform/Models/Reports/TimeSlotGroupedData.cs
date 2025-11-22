using System;
using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports
{
    public class TimeSlotGroupedData
    {
        public TimeSpan EventTime { get; set; }
        public List<ActorData> ActorsData { get; set; } = new();
        public int TotalTimeSlotPhotoCount { get; set; }
        public decimal TotalTimeSlotPhotoPrice { get; set; }
    }
}