using System;
using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports
{
    public class DateGroupedData
    {
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        
        public TimeSpan EventTime { get; set; }
        public List<TimeSlotGroupedData> TimeSlotsData { get; set; } = new();
        public int TotalDayPhotoCount { get; set; }
        public decimal TotalDayPhotoPrice { get; set; }
    }
}