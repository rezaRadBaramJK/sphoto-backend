using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Event
{
    public class EventDetailActor
    {
        public Actor Actor { get; set; }
        
        public decimal CameraManEachPictureCost { get; set; }
        
        public decimal CustomerMobileEachPictureCost { get; set; }
    }
}