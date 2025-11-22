using Nop.Core;
using Nop.Core.Domain.Common;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class ProductionEvent: BaseEntity, ISoftDeletedEntity
    {
        public bool Deleted { get; set; }
        public int EventId { get; set; }
        public int CustomerId { get; set; }
        public bool Active { get; set; }
    }
}