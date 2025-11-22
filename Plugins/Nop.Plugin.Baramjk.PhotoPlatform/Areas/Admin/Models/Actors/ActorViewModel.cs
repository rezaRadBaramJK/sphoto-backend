using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Actors
{
    public record ActorViewModel : BaseNopEntityModel, ILocalizedModel<ActorLocalizedModel>
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Name")]
        public string Name { get; set; }


        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.DefaultCameraManEachPictureCost")]
        public decimal DefaultCameraManEachPictureCost { get; set; }


        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.DefaultCustomerMobileEachPictureCost")]
        public decimal DefaultCustomerMobileEachPictureCost { get; set; }


        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.CustomerId")]
        public int CustomerId { get; set; }
        
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.CardNumber")]
        public string CardNumber { get; set; }
        
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.CardHolderName")]
        public string CardHolderName { get; set; }

        public IList<ActorLocalizedModel> Locales { get; set; } = new List<ActorLocalizedModel>();

        public ActorPictureSearchModel ActorPictureSearchModel { get; set; } = new ActorPictureSearchModel();

        public ActorPictureItemModel AddActorPictureItemModel { get; set; } = new ActorPictureItemModel();
    }
}