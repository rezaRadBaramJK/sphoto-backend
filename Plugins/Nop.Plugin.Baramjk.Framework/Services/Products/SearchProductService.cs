using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Baramjk.Framework.Services.Products
{
    public class SearchProductService : ISearchProductService
    {
        private readonly IAclService _aclService;
        private readonly IRepository<Category> _categoryRepository;
        private readonly ICategoryService _categoryService;
        private readonly ILanguageService _languageService;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        private readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        private readonly IRepository<ProductProductTagMapping> _productTagMappingRepository;
        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;

        public SearchProductService(IAclService aclService, ILanguageService languageService,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository, IRepository<Product> productRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<ProductProductTagMapping> productTagMappingRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            IStoreMappingService storeMappingService, IWorkContext workContext,
            IRepository<ProductAttributeMapping> productAttributeMappingRepository,
            IRepository<Category> categoryRepository, ICategoryService categoryService)
        {
            _aclService = aclService;
            _languageService = languageService;
            _localizedPropertyRepository = localizedPropertyRepository;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productRepository = productRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _productTagMappingRepository = productTagMappingRepository;
            _productTagRepository = productTagRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _productAttributeMappingRepository = productAttributeMappingRepository;
            _categoryRepository = categoryRepository;
            _categoryService = categoryService;
            _workContext = workContext;
        }

        public virtual async Task<IPagedList<Product>> SearchProductsAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = true,
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
            bool searchInAllLanguage = true
        )
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = _productRepository.Table;
            if (productIds != null && productIds.Any())
                productsQuery = productsQuery.Where(p => productIds.Contains(p.Id));

            if (!showHidden)
                productsQuery = productsQuery.Where(p => p.Published);
            else if (overridePublished.HasValue)
                productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

            if (createdFromUtc.HasValue)
                productsQuery = productsQuery.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);

            if (createdToUtc.HasValue)
                productsQuery = productsQuery.Where(o => createdToUtc.Value >= o.CreatedOnUtc);

            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

            //apply ACL constraints
            if (!showHidden)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
            }

            productsQuery =
                from p in productsQuery
                where !p.Deleted &&
                      (!visibleIndividuallyOnly || p.VisibleIndividually) &&
                      (vendorId == 0 || p.VendorId == vendorId) &&
                      (
                          warehouseId == 0 ||
                          (
                              !p.UseMultipleWarehouses
                                  ? p.WarehouseId == warehouseId
                                  : _productWarehouseInventoryRepository.Table.Any(pwi =>
                                      pwi.Id == warehouseId && pwi.ProductId == p.Id)
                          )
                      ) &&
                      (productType == null || p.ProductTypeId == (int)productType) &&
                      (showHidden || DateTime.UtcNow.Between(p.AvailableStartDateTimeUtc ?? DateTime.MinValue,
                          p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)) &&
                      (priceMin == null || p.Price >= priceMin) &&
                      (priceMax == null || p.Price <= priceMax)
                select p;

            if (!string.IsNullOrEmpty(keywords))
            {
                var langs = await _languageService.GetAllLanguagesAsync(true);

                //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
                var searchLocalizedValue = (languageId > 0 || searchInAllLanguage) && langs.Count >= 2 &&
                                           (showHidden || langs.Count(l => l.Published) >= 2);

                IQueryable<int> productsByKeywords;

                productsByKeywords =
                    from p in _productRepository.Table
                    where p.Name.Contains(keywords) ||
                          (searchDescriptions &&
                           (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords))) ||
                          (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                          (searchSku && p.Sku == keywords)
                    select p.Id;

                //search by SKU for ProductAttributeCombination
                if (searchSku)
                    productsByKeywords = productsByKeywords.Union(
                        from pac in _productAttributeCombinationRepository.Table
                        where pac.Sku == keywords
                        select pac.ProductId);

                if (searchProductTags)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join pt in _productTagRepository.Table on pptm.ProductTagId equals pt.Id
                        where pt.Name == keywords
                        select pptm.ProductId
                    );

                    if (searchLocalizedValue)
                        productsByKeywords = productsByKeywords.Union(
                            from pptm in _productTagMappingRepository.Table
                            join lp in _localizedPropertyRepository.Table on pptm.ProductTagId equals lp.EntityId
                            where lp.LocaleKeyGroup == nameof(ProductTag) &&
                                  lp.LocaleKey == nameof(ProductTag.Name) &&
                                  lp.LocaleValue.Contains(keywords)
                            select lp.EntityId);
                }

                if (searchLocalizedValue)
                    productsByKeywords = productsByKeywords.Union(
                        from lp in _localizedPropertyRepository.Table
                        let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                        lp.LocaleValue.Contains(keywords)
                        let checkShortDesc = searchDescriptions &&
                                             lp.LocaleKey == nameof(Product.ShortDescription) &&
                                             lp.LocaleValue.Contains(keywords)
                        let checkProductTags = searchProductTags &&
                                               lp.LocaleKeyGroup == nameof(ProductTag) &&
                                               lp.LocaleKey == nameof(ProductTag.Name) &&
                                               lp.LocaleValue.Contains(keywords)
                        where
                            (lp.LocaleKeyGroup == nameof(Product) &&
                             (searchInAllLanguage || lp.LanguageId == languageId) &&
                             (checkName || checkShortDesc)) || checkProductTags
                        select lp.EntityId);

                productsQuery =
                    from p in productsQuery
                    from pbk in productsByKeywords.InnerJoin(pbk => pbk == p.Id)
                    select p;
            }

            if (categoryIds is not null)
            {
                if (categoryIds.Contains(0))
                    categoryIds.Remove(0);

                if (categoryIds.Any())
                {
                    var productCategoryQuery =
                        from pc in _productCategoryRepository.Table
                        where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                              categoryIds.Contains(pc.CategoryId)
                        group pc by pc.ProductId
                        into pc
                        select new
                        {
                            ProductId = pc.Key, pc.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Name
                        select p;
                }
            }

            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
                              manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId
                        into pm
                        select new
                        {
                            ProductId = pm.Key, pm.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pm in productManufacturerQuery on p.Id equals pm.ProductId
                        orderby pm.DisplayOrder, p.Name
                        select p;
                }
            }

            if (productTagIds?.Any() == true)
                productsQuery =
                    from p in productsQuery
                    join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                    where productTagIds.Contains(ptm.ProductTagId)
                    select p;

            if (attributeIds?.Any() == true)
                productsQuery =
                    from p in productsQuery
                    join ptm in _productAttributeMappingRepository.Table on p.Id equals ptm.ProductId
                    where attributeIds.Contains(ptm.ProductAttributeId)
                    select p;

            if (excludeProductTagIds?.Any() == true)
                productsQuery = from p in productsQuery
                    join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId into mappings
                    from mapping in mappings.DefaultIfEmpty()
                    where mapping == null || excludeProductTagIds.Contains(mapping.ProductTagId) == false
                    select p;

            if (excludeCategoryIds is not null)
            {
                if (excludeCategoryIds.Contains(0))
                    excludeCategoryIds.Remove(0);

                if (excludeCategoryIds.Any())
                    productsQuery = from p in productsQuery
                        join ptm in _productCategoryRepository.Table on p.Id equals ptm.ProductId into mappings
                        from mapping in mappings.DefaultIfEmpty()
                        where mapping == null || excludeCategoryIds.Contains(mapping.CategoryId) == false
                        select p;
            }

            if (excludeAttributeIds?.Any() == true)
                productsQuery = from p in productsQuery
                    join ptm in _productAttributeMappingRepository.Table on p.Id equals ptm.ProductId into mappings
                    from mapping in mappings.DefaultIfEmpty()
                    where mapping == null || excludeAttributeIds.Contains(mapping.ProductAttributeId) == false
                    select p;

            if (excludeSpecificationAttributeOptions?.Any() == true)
                productsQuery = from p in productsQuery
                    join sp in _productSpecificationAttributeRepository.Table on p.Id equals sp.ProductId into mappings
                    from mapping in mappings.DefaultIfEmpty()
                    where mapping == null ||
                          excludeSpecificationAttributeOptions.Contains(mapping.SpecificationAttributeOptionId) == false
                    select p;


            if (filteredSpecOptions?.Any() == true)
            {
                var specificationAttributeIds = filteredSpecOptions
                    .Select(sao => sao.SpecificationAttributeId)
                    .Distinct();

                foreach (var specificationAttributeId in specificationAttributeIds)
                {
                    var optionIdsBySpecificationAttribute = filteredSpecOptions
                        .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                        .Select(o => o.Id);

                    var productSpecificationQuery =
                        from psa in _productSpecificationAttributeRepository.Table
                        where optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    productsQuery =
                        from p in productsQuery
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            return await productsQuery.OrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IPagedList<Product>> SearchProductsAsync(SearchProductModel request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var specificationAttributeOptions = GetSpecificationAttributeOptions();

            await CheckCategoryName();

            await CheckSubCategories();

            var productTagIds = request.ProductTagId > 0
                ? new List<int> { request.ProductTagId }
                : request.ProductTagIds;

            //products
            var products = await SearchProductsAsync(
                Math.Max(0, request.PageNumber - 1),
                request.PageSize,
                priceMin: request?.Price?.From,
                priceMax: request?.Price?.To,
                manufacturerIds: request.ManufacturerIds,
                categoryIds: request.CategoriesIds,
                vendorId: request.VendorId,
                orderBy: request.OrderBy,
                keywords: request.KeyWord,
                productTagIds: productTagIds,
                searchSku: request.SearchInSku,
                storeId: request.StoreId,
                productType: request.ProductType,
                overridePublished: request.Publish,
                showHidden: request.ShowHidden,
                filteredSpecOptions: specificationAttributeOptions,
                createdFromUtc: request.CreatedFromUtc,
                createdToUtc: request.CreatedToUtc,
                attributeIds: request.AttributeIds,
                excludeProductTagIds: request.ExcludeProductTagIds,
                excludeCategoryIds: request.ExcludeCategoryIds,
                excludeSpecificationAttributeOptions: request.ExcludeSpecificationAttributeOptions,
                excludeAttributeIds: request.ExcludeAttributeIds,
                productIds: request.ProductIds
            );

            return products;

            async Task CheckCategoryName()
            {
                if (string.IsNullOrEmpty(request.CategoryName) == false)
                {
                    request.CategoriesIds ??= new List<int>();

                    var category = await _categoryRepository.Table
                        .FirstOrDefaultAsync(item => item.Name == request.CategoryName);

                    if (category != null)
                        request.CategoriesIds.Add(category.Id);
                }
            }

            List<SpecificationAttributeOption> GetSpecificationAttributeOptions()
            {
                return request.FilteredSpecOptions?
                    .Select(item => new SpecificationAttributeOption
                    {
                        Id = item.SpecificationAttributeOptionId,
                        SpecificationAttributeId = item.SpecificationAttributeId
                    }).ToList();
            }

            async Task CheckSubCategories()
            {
                if (request.SubCategories)
                {
                    request.CategoriesIds ??= new List<int>();

                    var subCategories = new List<int>();
                    foreach (var categoriesId in request.CategoriesIds)
                    {
                        var ids = await _categoryService.GetChildCategoryIdsAsync(categoriesId);
                        subCategories.AddRange(ids);
                    }

                    request.CategoriesIds.AddRange(subCategories);
                    request.CategoriesIds = request.CategoriesIds.Distinct().ToList();
                }
            }
        }
    }
}