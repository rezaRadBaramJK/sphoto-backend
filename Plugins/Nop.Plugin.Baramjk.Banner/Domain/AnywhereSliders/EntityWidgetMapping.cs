using Nop.Core;

namespace Nop.Plugin.Baramjk.Banner.Domain.AnywhereSliders
{
    public class EntityWidgetMapping : BaseEntity
    {
        public int EntityType { get; set; }
        public int EntityId { get; set; }
        public string WidgetZone { get; set; }
        public int DisplayOrder { get; set; }
    }
}