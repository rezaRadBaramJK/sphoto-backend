using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using Nop.Services.Catalog;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.FrontendApi.Factories
{
    public class WishlistFactory
    {
        private readonly IProductDtoFactory _productDtoFactory;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;

        public WishlistFactory(IProductService productService, IShoppingCartService shoppingCartService,
            IStoreContext storeContext, IProductDtoFactory productDtoFactory)
        {
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _productDtoFactory = productDtoFactory;
        }

        public virtual async Task<IEnumerable<ProductOverviewDto>> PrepareSavedProductsAsync(
            Customer customer, GetSavedProductRequest request)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist,
                (await _storeContext.GetCurrentStoreAsync()).Id);

            var productIds = cart.Select(item => item.ProductId).Distinct().ToArray();
            var products = await _productService.GetProductsByIdsAsync(productIds);
            var overviewModels = await _productDtoFactory.PrepareProductOverviewAsync(products,
                prepareSpecifications: true);

            if (string.IsNullOrEmpty(request.CategoryName))
                request.CategoryName = null;

            if (request.CategoryId == null && request.CategoryName == null)
                return overviewModels;

            overviewModels = overviewModels.Where(item =>
                (request.CategoryId == null || item.Categories.Any(c => c.Id == request.CategoryId)) &&
                (request.CategoryName == null || item.Categories.Any(c => c.Name == request.CategoryName))
            ).ToList();

            return overviewModels;
        }
    }

    public class GetSavedProductRequest
    {
        public string CategoryName { get; set; }

        public int? CategoryId { get; set; }

        public bool PrepareSpecificationAttributes { get; set; }
    }
}