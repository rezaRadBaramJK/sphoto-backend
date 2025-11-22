using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Domains;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services
{
    public class GetVendorTimeSlotsResults
    {
        public Technician Technician { get; set; }
        
        public IList<ProductTimeSlotsResults> ProductTimeSlots { get; set; }
    }
}