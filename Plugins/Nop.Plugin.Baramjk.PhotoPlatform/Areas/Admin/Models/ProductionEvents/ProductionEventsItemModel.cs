using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ProductionEvents
{
    public record ProductionEventsItemModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ProductionEvents.EventId")]
        public int EventId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ProductionEvents.CustomerId")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ProductionEvents.ProductionEmail")]
        public string ProductionEmail { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ProductionEvents.ProductionPictureUrl")]
        public string ProductionPictureUrl { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ProductionEvents.Active")]
        public bool Active { get; set; }
    }
}