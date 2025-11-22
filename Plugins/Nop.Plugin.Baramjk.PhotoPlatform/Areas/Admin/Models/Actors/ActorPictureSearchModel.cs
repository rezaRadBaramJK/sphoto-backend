using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Actors
{
    public record ActorPictureSearchModel : BaseSearchModel
    {
        public int ActorId { get; set; }
    }
}