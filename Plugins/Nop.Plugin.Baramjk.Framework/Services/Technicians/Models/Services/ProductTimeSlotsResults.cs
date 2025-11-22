using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services
{
    public class ProductTimeSlotsResults
    {
        public Product Product { get; set; }
        
        public IList<TimeSlot> TimeSlot { get; set; }
    }
}