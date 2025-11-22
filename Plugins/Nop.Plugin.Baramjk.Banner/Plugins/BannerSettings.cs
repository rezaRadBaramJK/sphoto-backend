using Nop.Core.Configuration;

namespace Nop.Plugin.Baramjk.Banner.Plugins
{
    public class BannerSettings : ISettings
    {
        public string Tags { get; set; }

        public string ProductTags { get; set; }

        public string CategoryTags { get; set; }

        public string VendorTags { get; set; }
        
        public bool ShowInProductAttributeValuePage { get; set; }
    }
}