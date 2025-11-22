using Nop.Core;
using Nop.Core.Domain.Common;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class ShoppingCartItemTimeSlot : BaseEntity, ISoftDeletedEntity
    {
        public int TimeSlotId { get; set; }

        public int ShoppingCartItemId { get; set; }

        public int ActorId { get; set; }

        public int CameraManPhotoCount { get; set; }

        public int CustomerMobilePhotoCount { get; set; }

        public bool Deleted { get; set; }
    }
}