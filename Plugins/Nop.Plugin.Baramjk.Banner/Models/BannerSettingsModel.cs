using Nop.Core.Configuration;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Banner.Models
{
    public class BannerSettingsModel : ISettings
    {
        public string Tags { get; set; }

        public string ProductTags { get; set; }

        public string CategoryTags { get; set; }

        public string VendorTags { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Banner.Admin.ShowInProductAttributeValuePage")]
        public bool ShowInProductAttributeValuePage { get; set; }
    }
}