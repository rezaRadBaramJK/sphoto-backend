using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Product;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Baramjk.FrontendApi.Services;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.FrontendApi.Factories
{
    public class WebApiCatalogModelFactory : IWebApiCatalogModelFactory
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;
        private readonly IPictureService _pictureService;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IProductDtoFactory _productDtoFactory;
        private readonly IRepository<Product> _productRepository;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly FrontendCategoryService _frontendCategoryService;
        private readonly IProductTagService _productTagService;
        private readonly IVendorService _vendorService;
        private readonly IDispatcherService _dispatcherService;

        public WebApiCatalogModelFactory(IRepository<Category> categoryRepository, ICategoryService categoryService,
            IProductService productService, ILocalizationService localizationService, MediaSettings mediaSettings,
            IPictureService pictureService, IStaticCacheManager staticCacheManager, IStoreContext storeContext,
            IUrlRecordService urlRecordService, IWebHelper webHelper, IWorkContext workContext,
            IRepository<ProductCategory> productCategoryRepository, IRepository<Product> productRepository,
            IProductDtoFactory productDtoFactory, FrontendCategoryService frontendCategoryService,
            IProductTagService productTagService, IVendorService vendorService, IDispatcherService dispatcherService)
        {
            _categoryRepository = categoryRepository;
            _categoryService = categoryService;
            _productService = productService;
            _localizationService = localizationService;
            _mediaSettings = mediaSettings;
            _pictureService = pictureService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _productCategoryRepository = productCategoryRepository;
            _productRepository = productRepository;
            _productDtoFactory = productDtoFactory;
            _frontendCategoryService = frontendCategoryService;
            _productTagService = productTagService;
            _vendorService = vendorService;
            _dispatcherService = dispatcherService;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var models = await CreateCategoryDtoAsync(categories);
            return models;
        }

        public async Task<List<CategoryDto>> GetHomepageCategoriesAsync(bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0)
        {
            var categories = await _categoryService.GetAllCategoriesDisplayedOnHomepageAsync();
            var models = await CreateCategoryDtoAsync(categories,
                featuredProduct: featuredProduct, productCount: productCount, subCategoriesLevel: subCategoriesLevel);

            return models;
        }

        public async Task<List<CategoryDto>> GetRootCategoriesAsync(bool featuredProduct = false, int productCount = 0,
            int subCategoriesLevel = 0)
        {
            var categories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(0);
            var models = await CreateCategoryDtoAsync(categories,
                featuredProduct: featuredProduct, productCount: productCount, subCategoriesLevel: subCategoriesLevel);
            return models;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync(int[] ids, bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0)
        {
            var categories = await _categoryRepository.Table.Where(item => ids.Contains(item.Id)).ToListAsync();
            var categoryModel = await CreateCategoryDtoAsync(categories,
                featuredProduct: featuredProduct, productCount: productCount, subCategoriesLevel: subCategoriesLevel);

            return categoryModel;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync(List<string> name, bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0)
        {
            var categories = await _categoryRepository.Table.Where(item => name.Contains(item.Name)).ToListAsync();
            var categoryModel = await CreateCategoryDtoAsync(categories,
                featuredProduct: featuredProduct, productCount: productCount, subCategoriesLevel: subCategoriesLevel);

            return categoryModel;
        }

        public async Task<GetSubCategoryDto> GetSubCategoriesAsync(string parentName, bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0)
        {
            var category = await _categoryRepository.Table.FirstOrDefaultAsync(item => item.Name == parentName);
            if (category == null)
                return new GetSubCategoryDto();

            var categoryModel = await CreateCategoryDtoAsync(category,
                featuredProduct: featuredProduct, productCount: productCount, subCategoriesLevel: subCategoriesLevel);

            var subCategories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(category.Id);
            var subCategoryModels = await CreateCategoryDtoAsync(subCategories,
                featuredProduct: featuredProduct, productCount: productCount, subCategoriesLevel: subCategoriesLevel);

            var getSubCategoryDto = new GetSubCategoryDto
            {
                Category = categoryModel,
                SubCategories = subCategoryModels
            };

            return getSubCategoryDto;
        }

        public async Task<List<CategoryDto>> GetSubCategoriesAsync(int parentId, bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0)
        {
            var categories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(parentId);
            var models = await CreateCategoryDtoAsync(categories,
                featuredProduct: featuredProduct, productCount: productCount, subCategoriesLevel: subCategoriesLevel);
            return models;
        }

        public async Task<List<CategoryDto>> GetCategoriesByProductIdsAsync(int[] productIds)
        {
            var dictionary = await _categoryService.GetProductCategoryIdsAsync(productIds);
            var categoryIds = dictionary.SelectMany(item => item.Value).Distinct().ToArray();
            var categories = await _categoryService.GetCategoriesByIdsAsync(categoryIds);
            var categoryModels = await CreateCategoryDtoAsync(categories);
            return categoryModels;
        }

        public async Task<List<CategoryDto>> GetCategoriesByVendorIdAsync(int vendorId, int productCount = 0)
        {
            var query = from product in _productRepository.Table
                join productCategory in _productCategoryRepository.Table on product.Id equals productCategory.ProductId
                where product.VendorId == vendorId
                select productCategory.CategoryId;

            var categoryIds = await query.Distinct().ToArrayAsync();
            var categories = await _categoryService.GetCategoriesByIdsAsync(categoryIds);
            categories = categories.OrderBy(item => item.DisplayOrder).ToList();
            var categoryModels = await CreateCategoryDtoAsync(categories, productCount: productCount);
            foreach (var categoryDto in categoryModels)
            {
                await _dispatcherService.PublishAsync(FrameworkDefaultValues.ProductItemsEventTopic, categoryDto.Products);
            }

            
            return categoryModels;
        }

        public async Task<List<CategoryBreadCrumbDto>> GetCategoryBreadCrumbsAsync(int? parentCategoryId = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var breadCrumbs = await categories
                .Where(item => parentCategoryId == null || item.ParentCategoryId == parentCategoryId)
                .SelectAwait(async item =>
                {
                    var breadCrumb = await _categoryService.GetFormattedBreadCrumbAsync(item);
                    var categoryBreadCrumbDto = new CategoryBreadCrumbDto
                    {
                        BreadCrumb = breadCrumb,
                        Id = item.Id,
                        Name = item.Name,
                        ParentId = item.ParentCategoryId
                    };

                    return categoryBreadCrumbDto;
                }).ToListAsync();

            breadCrumbs = breadCrumbs.OrderBy(itm => itm.BreadCrumb)
                .ToList();

            return breadCrumbs;
        }

        public async Task<CategoryDto> CreateCategoryDtoAsync(Category category, int pictureSize = 0,
            Language language = null, Store store = null, bool featuredProduct = false, int productCount = 0,
            int subCategoriesLevel = 0)
        {
            language ??= await _workContext.GetWorkingLanguageAsync();
            store ??= await _storeContext.GetCurrentStoreAsync();
            if (pictureSize == 0)
                pictureSize = _mediaSettings.CategoryThumbPictureSize;

            var model = new CategoryDto
            {
                Id = category.Id,
                Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                Description = await _localizationService.GetLocalizedAsync(category, x => x.Description),
                MetaKeywords = await _localizationService.GetLocalizedAsync(category, x => x.MetaKeywords),
                MetaDescription =
                    await _localizationService.GetLocalizedAsync(category, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(category, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(category),
                ParentCategoryId = category.ParentCategoryId
            };

            model.PictureModel = await PreparePictureModel(pictureSize, language, store, category);

            if (featuredProduct)
            {
                var featuredProducts = await _productService.GetCategoryFeaturedProductsAsync(category.Id, store.Id);
                model.FeaturedProducts =
                    await _productDtoFactory.PrepareProductOverviewAsync(featuredProducts);
            }

            if (productCount > 0)
            {
                // var productCategories = await _categoryService.GetProductCategoriesByCategoryIdAsync(category.Id,
                //     pageSize: productCount);
                // var productIds = productCategories.Select(i => i.ProductId).ToArray();
                // var products = await _productService.GetProductsByIdsAsync(productIds);

                //! custom logic
                var categoryProducts =
                    await _frontendCategoryService.GetProductsByCategoryIdAsync(category.Id, productCount);
                model.Products = await _productDtoFactory.PrepareProductOverviewAsync(categoryProducts);
                //! end
            }

            if (subCategoriesLevel > 0)
            {
                var categories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(category.Id);
                var categoryModels = await categories.SelectAwait(async cat =>
                        await CreateCategoryDtoAsync(cat, pictureSize, language, store, featuredProduct, productCount,
                            subCategoriesLevel - 1))
                    .ToListAsync();

                model.SubCategories = categoryModels;
            }

            return model;
        }

        public async Task<IList<SubCategoryModelDto>> PrepareSubCategoriesLevelDtoAsync(int parentId,
            int subCategoryLevel)
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;

            var subCategories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(parentId);
            var result = new List<SubCategoryModelDto>();
            foreach (var sub in subCategories)
            {
                var dto = new SubCategoryModelDto
                {
                    Id = sub.Id,
                    Name = sub.Name,
                    Description = sub.Description,
                    SeName = await _urlRecordService.GetSeNameAsync(sub),
                    PictureModel =
                        (await PreparePictureModel(pictureSize, language, store, sub)).ToDto<PictureModelDto>(),
                };
                if (subCategoryLevel > 1)
                {

                    dto.SubCategories = await PrepareSubCategoriesLevelDtoAsync(sub.Id, subCategoryLevel - 1);
                }

                result.Add(dto);
            }

            return result;
        }
        
        public async Task<IList<ProductTagModelDto>> PrepareProductTagDtoAsync(int productId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var productsTags = await _productTagService.GetAllProductTagsByProductIdAsync(productId);

            var model = await productsTags
                //filter by store
                .WhereAwait(async x => await _productTagService.GetProductCountByProductTagIdAsync(x.Id, store.Id) > 0)
                .SelectAwait(async x => new ProductTagModelDto
                {
                    Id = x.Id,
                    Name = await _localizationService.GetLocalizedAsync(x, y => y.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(x),
                    ProductCount = await _productTagService.GetProductCountByProductTagIdAsync(x.Id, store.Id)
                }).ToListAsync();

            return model;
        }



        public async Task<List<CategoryDto>> CreateCategoryDtoAsync(IList<Category> categories, int pictureSize = 0,
            Language language = null, Store store = null, bool featuredProduct = false, int productCount = 0,
            int subCategoriesLevel = 0)
        {
            language ??= await _workContext.GetWorkingLanguageAsync();
            store ??= await _storeContext.GetCurrentStoreAsync();
            if (pictureSize == 0)
                pictureSize = _mediaSettings.CategoryThumbPictureSize;

            var categoryModels = await categories.SelectAwait(async category =>
                    await CreateCategoryDtoAsync(category, pictureSize, language, store, featuredProduct, productCount,
                        subCategoriesLevel))
                .ToListAsync();

            return categoryModels;
        }

        public async Task<PictureModel> PreparePictureModel(Category category)
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;
            return await PreparePictureModel(pictureSize, language, store, category);
        }


    private async Task<PictureModel> PreparePictureModel(int pictureSize, Language language, Store store,
            Category category)
        {
            //prepare picture model
            var secured = _webHelper.IsCurrentConnectionSecured();
            var categoryPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                NopModelCacheDefaults.CategoryPictureModelKey, category, pictureSize, true, language, secured, store);

            var pictureModel = await _staticCacheManager.GetAsync(categoryPictureCacheKey, async () =>
            {
                var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
                string fullSizeImageUrl, imageUrl;

                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                var titleLocale =
                    await _localizationService.GetResourceAsync("Media.Category.ImageLinkTitleFormat");
                var altLocale =
                    await _localizationService.GetResourceAsync("Media.Category.ImageAlternateTextFormat");
                return new PictureModel
                {
                    FullSizeImageUrl = fullSizeImageUrl,
                    ImageUrl = imageUrl,
                    Title = string.Format(titleLocale, category.Name),
                    AlternateText = string.Format(altLocale, category.Name)
                };
            });

            return pictureModel;
        }
    
        public async Task<VendorBriefInfoModelDto> PrepareVendorBriefInfoModelAsync(int vendorId)
        {
            if (vendorId <= 0)
                return null;
            
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (vendor is { Deleted: false, Active: true })
            {
                return new VendorBriefInfoModelDto()
                {
                    Id = vendor.Id,
                    Name = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(vendor),
                };
            }

            return null;
        }
    }
}