using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ActorEvents
{
    public record BulkSharesEditViewModel : BaseNopModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.EventId")]
        public int EventId { get; set; }


        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ActorShare")]
        public decimal ActorShare { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ProductionShare")]
        public decimal ProductionShare { get; set; }
    }
}