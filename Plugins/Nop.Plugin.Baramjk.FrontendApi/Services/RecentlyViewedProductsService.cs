using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Common;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class RecentlyViewedProductsService : IRecentlyViewedProductsService
    {
        private const string RecentlyViewedProductsKey = "RecentlyViewedProducts";

        private readonly CatalogSettings _catalogSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;

        public RecentlyViewedProductsService(CatalogSettings catalogSettings, IProductService productService,
            IWorkContext workContext, IGenericAttributeService genericAttributeService)
        {
            _catalogSettings = catalogSettings;
            _productService = productService;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
        }

        public virtual async Task<IList<Product>> GetRecentlyViewedProductsAsync(int number)
        {
            var productIds = await GetRecentlyViewedProductsIdsAsync(number);
            var products = await _productService.GetProductsByIdsAsync(productIds.ToArray());
            return products.Where(product => product.Published && !product.Deleted).ToList();
        }

        public virtual async Task AddProductToRecentlyViewedListAsync(int productId)
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return;

            var productIds = await GetRecentlyViewedProductsIdsAsync(int.MaxValue);

            if (productIds.Contains(productId))
                return;

            productIds.Insert(0, productId);

            productIds = productIds.Take(_catalogSettings.RecentlyViewedProductsNumber).ToList();
            var ids = string.Join(",", productIds);

            var customer = await _workContext.GetCurrentCustomerAsync();
            await _genericAttributeService.SaveAttributeAsync(customer, RecentlyViewedProductsKey, ids);
        }

        protected async Task<List<int>> GetRecentlyViewedProductsIdsAsync(int number)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var attrValue =
                await _genericAttributeService.GetAttributeAsync(customer, RecentlyViewedProductsKey, 0, "");

            var productIds = attrValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return productIds.Select(int.Parse).Distinct().Take(number).ToList();
        }
    }
}