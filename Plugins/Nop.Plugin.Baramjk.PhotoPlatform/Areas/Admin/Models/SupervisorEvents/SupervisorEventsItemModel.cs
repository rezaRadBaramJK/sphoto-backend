using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.SupervisorEvents
{
    public record SupervisorEventsItemModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.SupervisorEvents.EventId")]
        public int EventId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.SupervisorEvents.CustomerId")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.SupervisorEvents.SupervisorEmail")]
        public string SupervisorEmail { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.SupervisorEvents.SupervisorPictureUrl")]
        public string SupervisorPictureUrl { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.SupervisorEvents.Active")]
        public bool Active { get; set; }
    }
}