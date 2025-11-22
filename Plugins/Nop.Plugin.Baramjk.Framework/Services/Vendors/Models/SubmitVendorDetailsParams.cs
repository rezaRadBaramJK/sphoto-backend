using System;
using System.Collections.Generic;
using Nop.Core.Domain.Vendors;

namespace Nop.Plugin.Baramjk.Framework.Services.Vendors.Models
{
    public class SubmitVendorDetailsParams
    {
        public Vendor Vendor { get; set; }
        
        public TimeSpan StartTime { get; set; }
        
        public TimeSpan EndTime { get; set; }

        public IList<int> OffDaysOfWeekIds { get; set; } = new List<int>();
        
        public bool IsAvailable { get; set; }
    }
}