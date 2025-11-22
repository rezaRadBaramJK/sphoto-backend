using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Banner.Services
{
    public class BannerTagModel
    {
        public string[] Tags { get; set; }

        public string[] ProductTags { get; set; }

        public string[] CategoryTags { get; set; }

        public string[] VendorTags { get; set; }
        public List<string> BannerUsedTags { get; set; }
        public List<string> AllTags { get; set; }
    }
}