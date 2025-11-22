using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reports
{
    public class ActorRevenueReportDetailsModel
    {
        public Order Order { get; set; }
        public List<ReservationItem> ReservationItems { get; set; }
        public ActorEvent ActorEvent { get; set; }
        public TimeSlot TimeSlot { get; set; }
    }
}