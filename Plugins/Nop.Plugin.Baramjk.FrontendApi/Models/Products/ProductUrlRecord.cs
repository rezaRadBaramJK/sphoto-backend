using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Seo;

namespace Nop.Plugin.Baramjk.FrontendApi.Models.Products
{
    public class ProductUrlRecordPair
    {
        public Product Product { get; set; }
        
        public UrlRecord UrlRecord { get; set; }
    }
}