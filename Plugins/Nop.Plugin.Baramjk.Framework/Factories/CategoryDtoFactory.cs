using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Baramjk.Framework.Models.Categories;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.Framework.Factories
{
    public class CategoryDtoFactory : ICategoryDtoFactory
    {
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;
        private readonly IPictureService _pictureService;
        private readonly IProductDtoFactory _productDtoFactory;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;

        public CategoryDtoFactory(ICategoryService categoryService, IProductService productService,
            ILocalizationService localizationService, MediaSettings mediaSettings, IPictureService pictureService,
            IStaticCacheManager staticCacheManager, IStoreContext storeContext, IUrlRecordService urlRecordService,
            IWebHelper webHelper, IWorkContext workContext, IProductDtoFactory productDtoFactory)
        {
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
            _productDtoFactory = productDtoFactory;
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
                var productCategories = await _categoryService.GetProductCategoriesByCategoryIdAsync(category.Id,
                    pageSize: productCount);
                var productIds = productCategories.Select(i => i.ProductId).ToArray();
                var products = await _productService.GetProductsByIdsAsync(productIds);
                model.Products = await _productDtoFactory.PrepareProductOverviewAsync(products);
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
    }
}