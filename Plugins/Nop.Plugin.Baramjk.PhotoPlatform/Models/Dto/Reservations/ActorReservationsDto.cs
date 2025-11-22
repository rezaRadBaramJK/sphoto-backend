using System;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Picture;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Reservations
{
    public class ActorReservationsDto : CamelCaseBaseDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public int CameramanPhotoCount { get; set; }
        public int CustomerMobilePhotoCount { get; set; }
        public PictureDto Picture { get; set; }
        public string TotalCommission { get; set; }
    }
}