using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.ShoppingCart
{
    public class ShoppingCartActorPhotoDetailsDto: CamelCaseModelWithIdDto
    {
        public int ActorId { get; set; }
        public string ActorName { get; set; }
        public int CameraManPhotoCount { get; set; }
        public int CustomerMobilePhotoCount { get; set; }
    }
}