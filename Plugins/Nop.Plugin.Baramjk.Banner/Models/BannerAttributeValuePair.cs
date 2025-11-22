using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Banner.Domain;

namespace Nop.Plugin.Baramjk.Banner.Models
{
    public class BannerAttributeValuePair
    {
        public ProductAttributeValue AttributeValue { get; set; }
        
        public BannerRecord Banner { get; set; }
    }
}