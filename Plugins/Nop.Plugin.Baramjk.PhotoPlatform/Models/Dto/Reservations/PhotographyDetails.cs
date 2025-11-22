using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Picture;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Reservations
{
    public class PhotographyDetails : CamelCaseBaseDto
    {
        public int ReservationId { get; set; }
        public int ActorId { get; set; }
        public string ActorName { get; set; }
        public int CameraManPhotoCount { get; set; }
        public int CustomerMobilePhotoCount { get; set; }
        public int UsedCameraManPhotoCount { get; set; }
        public int UsedCustomerMobilePhotoCount { get; set; }
        public string CameramanEachPictureCost { get; set; }
        public string CustomerMobileEachPictureCost { get; set; }
        public PictureDto ActorPicture { get; set; }
        public List<PictureDto> ActorPictures { get; set; }
    }
}