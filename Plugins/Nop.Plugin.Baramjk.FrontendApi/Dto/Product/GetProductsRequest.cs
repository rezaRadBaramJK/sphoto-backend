using Nop.Plugin.Baramjk.Framework.Services.Products;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Product
{
    public record GetProductsRequest : SearchProductModel
    {
        
        public bool IncludeAttributes { get; set; }
        
    }
}