using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ActorEventTimeSlots
{
    public record ActorEventTimeSlotsItemModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ActorName")]
        public string ActorName { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ActorPictureUrl")]
        public string ActorPictureUrl { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEventTimeSlots.Active")]
        public bool Active { get; set; }

        public int TimeSlotId { get; set; }

        public int ActorEventId { get; set; }
    }
}