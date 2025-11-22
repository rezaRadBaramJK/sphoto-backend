using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reports
{
    public class ActorReportDetailsModel
    {
        public Product Product { get; set; }
        public EventDetail EventDetail { get; set; }
        public List<TimeSlot> TimeSlots { get; set; }
    }
}