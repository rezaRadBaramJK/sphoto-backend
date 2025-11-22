using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Baramjk.Framework.Services.Products
{
    public interface ISearchProductService
    {
        Task<IPagedList<Product>> SearchProductsAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool excludeFeaturedProducts = false,
            decimal? priceMin = null,
            decimal? priceMax = null,
            IList<int> productTagIds = null,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<SpecificationAttributeOption> filteredSpecOptions = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            IList<int> productIds = null,
            IList<int> attributeIds = null,
            IList<int> excludeProductTagIds = null,
            IList<int> excludeCategoryIds = null,
            IList<int> excludeSpecificationAttributeOptions = null,
            IList<int> excludeAttributeIds = null,
            bool searchInAllLanguage=false
            );

        Task<IPagedList<Product>> SearchProductsAsync(SearchProductModel request);
    }
}