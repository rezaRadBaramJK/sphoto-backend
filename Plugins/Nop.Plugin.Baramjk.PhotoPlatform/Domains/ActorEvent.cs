using Nop.Core;
using Nop.Core.Domain.Common;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class ActorEvent : BaseEntity, ISoftDeletedEntity
    {
        public int EventId { get; set; }
        public int ActorId { get; set; }
        public decimal CameraManEachPictureCost { get; set; }
        public decimal CustomerMobileEachPictureCost { get; set; }
        public bool Deleted { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal ProductionShare { get; set; }
        public decimal ActorShare { get; set; }
        public int DisplayOrder { get; set; }
    }
}