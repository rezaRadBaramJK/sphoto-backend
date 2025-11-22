using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Picture;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.ActorEvent
{
    public class ActorEventDto : CamelCaseModelDto
    {
        public int ActorId { get; set; }
        public string ActorName { get; set; }
        public decimal CameraManEachPictureCost { get; set; }
        public decimal CustomerMobileEachPictureCost { get; set; }
        
        public PictureDto Picture { get; set; }
        
        public List<PictureDto> Pictures { get; set; }
    }
}