using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ActorEvents
{
    public record ActorEventsItemModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.EventId")]
        public int EventId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ActorId")]
        public int ActorId { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ActorName")]
        public string ActorName { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ActorPictureUrl")]
        public string ActorPictureUrl { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.CameraManEachPictureCost")]
        public decimal CameraManEachPictureCost { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.CustomerMobileEachPictureCost")]
        public decimal CustomerMobileEachPictureCost { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.CommissionAmount")]
        public decimal CommissionAmount { get; set; }
        
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ActorPhotoPrice")]
        public decimal ActorPhotoPrice { get; set; }
                
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.DisplayOrder")]
        public int DisplayOrder { get; set; }
        
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ActorShare")]
        public decimal ActorShare { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ProductionShare")]
        public decimal ProductionShare { get; set; }
    }
}