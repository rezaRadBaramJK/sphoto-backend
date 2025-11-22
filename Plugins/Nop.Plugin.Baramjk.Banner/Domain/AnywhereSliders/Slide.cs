using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Baramjk.Banner.Domain.AnywhereSliders
{
    public class Slide : BaseEntity, ILocalizedEntity
    {
        public string SystemName { get; set; }

        public string Url { get; set; }

        public string Alt { get; set; }

        public bool Visible { get; set; }

        public int DisplayOrder { get; set; }

        public int PictureId { get; set; }

        public int MobilePictureId { get; set; }

        public string Content { get; set; }

        public SlideType SlideType { get; set; }

        public int SliderId { get; set; }
    }
}