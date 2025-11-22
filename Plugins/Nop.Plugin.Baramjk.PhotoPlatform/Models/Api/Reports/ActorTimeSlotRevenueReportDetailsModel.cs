using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reports
{
    public class ActorTimeSlotRevenueReportDetailsModel
    {
        public EventDetail EventDetail { get; set; }

        public ReservationItem ReservationItem { get; set; }

        public ActorEvent ActorEvent { get; set; }

        public TimeSlot TimeSlot { get; set; }
        
        public Product Product { get; set; }
    }
}