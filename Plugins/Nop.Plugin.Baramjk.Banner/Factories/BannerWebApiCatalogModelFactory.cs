using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Baramjk.Banner.Domain;
using Nop.Plugin.Baramjk.Banner.Services;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models.Categories;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.Banner.Factories
{
    public class BannerWebApiCatalogModelFactory
    {
        private readonly ICategoryService _categoryService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly MediaSettings _mediaSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductService _productService;
        private readonly IProductDtoFactory _productDtoFactory;
        private readonly IWebHelper _webHelper;
        private readonly IPictureService _pictureService;
        private readonly BannerService _bannerService;

        public BannerWebApiCatalogModelFactory(
            ICategoryService categoryService,
            IWorkContext workContext,
            IStoreContext storeContext, 
            MediaSettings mediaSettings, 
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            IProductService productService, 
            IProductDtoFactory productDtoFactory, 
            IWebHelper webHelper, 
            IPictureService pictureService, 
            BannerService bannerService)
        {
            _categoryService = categoryService;
            _workContext = workContext;
            _storeContext = storeContext;
            _mediaSettings = mediaSettings;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _productService = productService;
            _productDtoFactory = productDtoFactory;
            _webHelper = webHelper;
            _pictureService = pictureService;
            _bannerService = bannerService;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var models = await CreateCategoryDtoAsync(categories);
            return models;
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
                ParentCategoryId = category.ParentCategoryId,
                PictureModel = await PreparePictureModel(pictureSize, category)
            };

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
        
        private async Task<PictureModel> PreparePictureModel(int pictureSize, Category category)
        {
            var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
            string fullSizeImageUrl, imageUrl;
            (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
            (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

            var titleLocale =
                await _localizationService.GetResourceAsync("Media.Category.ImageLinkTitleFormat");
            var altLocale =
                await _localizationService.GetResourceAsync("Media.Category.ImageAlternateTextFormat");
                
            var pictureModel = new PictureModel
            {
                FullSizeImageUrl = fullSizeImageUrl,
                ImageUrl = imageUrl,
                Title = string.Format(titleLocale, category.Name),
                AlternateText = string.Format(altLocale, category.Name),
            };
            if(category.ParentCategoryId == 0)
                await AddSvgIconAsync(pictureModel, category.Id);
            return pictureModel;
        }

        private async Task AddSvgIconAsync(PictureModel pictureModel, int categoryId)
        {
            var banners = await _bannerService.GetBannersAsync
            ( 
                entityName: "Category",
                entityId: categoryId,
                tag: "SvgIcon",
                bannerType: BannerType.Picture
            );

            var bannerRecord = banners.FirstOrDefault();
            if(bannerRecord == null)
                return;
            
            var svgIconUrl = $"{_webHelper.GetStoreLocation()}banner/GetFile/{bannerRecord.FileName}";
            pictureModel.ImageUrl = svgIconUrl;
            pictureModel.FullSizeImageUrl = svgIconUrl;
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
        
        
    }
}