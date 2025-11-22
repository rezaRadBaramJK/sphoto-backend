using System;
using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Helpers
{
    public static class TimeSpanHelper
    {
        
        public static TimeSpan[] GetTimeSpanIntervals(TimeSpan start, TimeSpan end, int duration)
        {
            var intervals = new List<TimeSpan>();
            var interval = TimeSpan.FromMinutes(duration);
            
            for (var current = start; current < end; current = current.Add(interval))
            {
                intervals.Add(current);
            }
    
            return intervals.ToArray();
        }
    }
}