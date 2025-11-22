using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ActorEventTimeSlots
{
    public record ActorEventTimeSlotsSearchModel : BaseSearchModel
    {
        public int TimeSlotId { get; set; }
        public DateTime TimeSlotDateTime { get; set; }
    }
}