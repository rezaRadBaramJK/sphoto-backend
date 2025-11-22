using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Picture;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.ShoppingCart
{
    public class ShoppingCartDetailsDto : CamelCaseModelWithIdDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public PictureDto Picture { get; set; }
        public string ReservationDate { get; set; }
        public string ReservationTime { get; set; }

        public int TimeSlotId { get; set; }
        public List<ShoppingCartActorPhotoDetailsDto> PhotographyDetails { get; set; }
    }
}