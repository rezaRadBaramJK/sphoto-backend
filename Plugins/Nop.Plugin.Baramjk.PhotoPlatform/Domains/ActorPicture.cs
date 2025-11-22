using Nop.Core;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class ActorPicture: BaseEntity
    {
        public int PictureId { get; set; }
        public int ActorId { get; set; }
        public int DisplayOrder { get; set; }
        
    }
}