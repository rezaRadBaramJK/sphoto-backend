using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Core.Models.Vendors
{
    public record VendorDetailsViewModel: BaseNopModel
    {
        public int VendorId { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Montajjat.Admin.Vendors.Details.StartTime")]
        public TimeSpan StartTime { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Montajjat.Admin.Vendors.Details.EndTime")]
        public TimeSpan EndTime { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Montajjat.Admin.Vendors.Details.OffDaysOfWeekIds")]
        public IList<int> OffDaysOfWeekIds { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Montajjat.Admin.Vendors.Details.IsAvailable")]
        public bool IsAvailable { get; set; }
        
        public IList<SelectListItem> AvailableDaysOfWeek { get; set; }
    }
}