using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Actors
{
    public record ActorPictureItemModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorPicture.ActorId")]
        public int ActorId { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorPicture.PictureId")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorPicture.PictureUrl")]
        public string PictureUrl { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorPicture.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}