using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;

namespace Nop.Plugin.Baramjk.Framework.Domain.Vendors
{
    public class VendorDetail: BaseEntity
    {
        public int VendorId { get; set; }
        
        public TimeSpan StartTime { get; set; }
        
        public TimeSpan EndTime { get; set; }
        
        public string OffDaysOfWeekIds { get; set; } = string.Empty;
        
        public bool IsAvailable { get; set; }

        public List<DayOfWeek> OffDaysOfWeeks
        {
            get => string.IsNullOrEmpty(OffDaysOfWeekIds)
                ? new List<DayOfWeek>()
                : OffDaysOfWeekIds.Split(",").Select(x => (DayOfWeek)int.Parse(x)).ToList();
            set => OffDaysOfWeekIds = string.Join(',', value.Select(x => (int)x));
        }
    }
}