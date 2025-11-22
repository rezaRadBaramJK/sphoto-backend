using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Actors
{
    public record ActorListItemModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Name")]
        public string Name { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Picture")]
        public string PictureUrl { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.DefaultCameraManEachPictureCost")]
        public decimal DefaultCameraManEachPictureCost  { get; set; }
        
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.DefaultCustomerMobileEachPictureCost")]
        public decimal DefaultCustomerMobileEachPictureCost  { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Email")]
        public string Email  { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.CardNumber")]
        public string CardNumber  { get; set; }
        
    }
}