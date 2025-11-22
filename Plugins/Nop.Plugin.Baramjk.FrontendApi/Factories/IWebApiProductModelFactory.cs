using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Product;

namespace Nop.Plugin.Baramjk.FrontendApi.Factories
{
    public interface IWebApiProductModelFactory
    {
        Task<ProductSearchResultDto> SearchProductsAsync(GetProductsRequest command);
        Task<ProductListDto> PrepareProductsAsync(GetProductsRequest command);
        Task<List<ProductOverviewDto>> PrepareProductsAsync(int[] productIds);
        Task<ProductListDto> PrepareMyProductsAsync(int vendorId, int pageNumber = 1, int pageSize = 25);
        Task<ProductListDto> PrepareProductsMostVisitedAsync(int pageNumber = 0, int pageSize = 25);

        Task<ProductDetailsModelDto> PrepareProductDetailsModelAsync(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false);
    }
}