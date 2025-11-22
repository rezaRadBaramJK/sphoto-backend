using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Actors
{
    public record CustomerDetailsWidgetModel : BaseNopModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CustomerDetailsWidget.ActorId")]
        public int ActorId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CustomerDetailsWidget.CustomerId")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CustomerDetailsWidget.ActorLink")]
        public string ActorLink { get; set; }
    }
}