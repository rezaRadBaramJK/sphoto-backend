using Nop.Core;
using Nop.Core.Domain.Stores;

namespace Nop.Plugin.Baramjk.Banner.Domain.AnywhereSliders
{
    public class Slider : BaseEntity, IStoreMappingSupported
    {
        public string SystemName { get; set; }

        public bool PreLoadFirstSlide { get; set; }

        public bool Autoplay { get; set; }

        public int AutoplaySpeed { get; set; }

        public int Speed { get; set; }

        public bool PauseOnHover { get; set; }

        public bool Fade { get; set; }

        public bool Dots { get; set; }

        public bool Arrows { get; set; }

        public int MobileBreakpoint { get; set; }

        public string CustomClass { get; set; }

        public int LanguageId { get; set; }

        public bool LimitedToStores { get; set; }
    }
}