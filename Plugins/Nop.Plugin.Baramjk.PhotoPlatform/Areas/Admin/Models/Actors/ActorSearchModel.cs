using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Actors
{
    public record ActorSearchModel : BaseSearchModel
    {
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorSearchModel.SearchName")]
        
        public string SearchName { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorSearchModel.SearchEmail")]
        public string SearchEmail { get; set; }
    }
}