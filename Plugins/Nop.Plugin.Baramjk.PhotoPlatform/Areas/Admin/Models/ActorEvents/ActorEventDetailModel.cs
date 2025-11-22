using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ActorEvents
{
    public class ActorEventDetailModel
    {
        public Actor Actor { get; set; }
        
        public ActorEvent ActorEvent { get; set; }
        
        public ActorPicture ActorPicture { get; set; }
    }
}