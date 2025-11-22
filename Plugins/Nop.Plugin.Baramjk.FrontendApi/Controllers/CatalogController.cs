using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments;
using Nop.Plugin.Baramjk.Framework.Services.Products;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog;
using Nop.Plugin.Baramjk.FrontendApi.Factories;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Baramjk.FrontendApi.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class CatalogController : BaseNopWebApiFrontendAllowAnonymousController
    {
        private readonly IAclService _aclService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly MediaSettings _mediaSettings;
        private readonly IPermissionService _permissionService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IVendorService _vendorService;
        private readonly VendorSettings _vendorSettings;
        private readonly IWebApiCatalogModelFactory _webApiCatalogModelFactory;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly FrontendCategoryService _frontendCategoryService;
        private readonly CategoryFactory _categoryFactory;
        private readonly IEntityAttachmentService _entityAttachmentService;
        private readonly IFavoriteProductService _favoriteProductService;
        private readonly IDispatcherService _dispatcherService;
        

        public CatalogController(CatalogSettings catalogSettings,
            IAclService aclService,
            ICatalogModelFactory catalogModelFactory,
            ICategoryService categoryService,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IPermissionService permissionService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductTagService productTagService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            VendorSettings vendorSettings,
            IWebApiCatalogModelFactory webApiCatalogModelFactory,
            FrontendCategoryService frontendCategoryService,
            CategoryFactory categoryFactory,
            IFavoriteProductService favoriteProductService, 
            IDispatcherService dispatcherService)
        {
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _catalogModelFactory = catalogModelFactory;
            _categoryService = categoryService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _permissionService = permissionService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _productTagService = productTagService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _vendorSettings = vendorSettings;
            _webApiCatalogModelFactory = webApiCatalogModelFactory;
            _frontendCategoryService = frontendCategoryService;
            _categoryFactory = categoryFactory;
            _favoriteProductService = favoriteProductService;
            _dispatcherService = dispatcherService;
            //! DO NOT INJECT BY CONSTRUCTOR ARGUMENT
            _entityAttachmentService = EngineContext.Current.Resolve<IEntityAttachmentService>();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<CategoryModel>), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetHomepageCategories(bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0)
        {
            var model = await _webApiCatalogModelFactory.GetHomepageCategoriesAsync(featuredProduct,
                productCount, subCategoriesLevel);

            foreach (var category in model)
            {
                await _dispatcherService.PublishAsync(FrameworkDefaultValues.ProductItemsEventTopic, category.Products);

            }
            
            return ApiResponseFactory.Success(model);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<CategoryModel>), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetRootCategories(bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0, bool includeBanners = false)
        {
            var model = await _webApiCatalogModelFactory.GetRootCategoriesAsync(featuredProduct,
                productCount, subCategoriesLevel);

            if (includeBanners)
            {
                foreach (var category in model)
                {
                    var banners = await _categoryFactory.PrepareBannersAsync(category.Id);
                    if (banners != null)
                        category.Banners = banners;
                }
            }

            return ApiResponseFactory.Success(model);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<CategoryModel>), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetAllCategoriesAsync()
        {
            var model = await _webApiCatalogModelFactory.GetAllCategoriesAsync();
            return ApiResponseFactory.Success(model);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<CategoryModel>), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetSubCategories(int categoryId, bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0)
        {
            var model = await _webApiCatalogModelFactory.GetSubCategoriesAsync(categoryId, featuredProduct,
                productCount, subCategoriesLevel);
            return ApiResponseFactory.Success(model);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<GetSubCategoryDto>), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetSubCategoriesByName(string name, bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0)
        {
            var model = await _webApiCatalogModelFactory.GetSubCategoriesAsync(name, featuredProduct,
                productCount, subCategoriesLevel);
            return ApiResponseFactory.Success(model);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<CategoryBreadCrumbDto>), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetBreadCrumbsAsync(int? parentCategoryId = null)
        {
            var model = await _webApiCatalogModelFactory.GetCategoryBreadCrumbsAsync(parentCategoryId);
            return ApiResponseFactory.Success(model);
        }

        [HttpPost]
        [ProducesResponseType(typeof(IList<GetSubCategoryDto>), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetCategoriesByNames([FromBody] GetCategoriesByNamesRequest request)
        {
            var model = await _webApiCatalogModelFactory.GetCategoriesAsync(request.Names, request.FeaturedProduct,
                request.ProductCount, request.SubCategoriesLevel);
            return ApiResponseFactory.Success(model);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<GetSubCategoryDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCategoriesByVendorId(int vendorId, int productCount = 0)
        {
            var categoryModels = await _webApiCatalogModelFactory.GetCategoriesByVendorIdAsync(vendorId, productCount);
            return ApiResponseFactory.Success(categoryModels);
        }


        #region Categories

        [HttpGet]
        public async Task<IActionResult> GetCategoriesProductsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            int productLimit = 10,
            bool prepareProductAttributes = false)
        {
            if (pageNumber <= 0)
                return ApiResponseFactory.BadRequest("Invalid page number.");

            if (pageSize <= 0)
                return ApiResponseFactory.BadRequest("Invalid page size.");

            if (productLimit <= 0)
                return ApiResponseFactory.BadRequest("Invalid product limit value.");

            var pageIndex = pageNumber - 1;
            var categoriesProducts =
                await _frontendCategoryService.GetCategoriesProductsAsync(pageIndex, pageSize, productLimit);
            var result =
                await _categoryFactory.PrepareCategoryProductsAsync(categoriesProducts, prepareProductAttributes);
            return ApiResponseFactory.Success(result);
        }

        /// <summary>
        ///     Get category
        /// </summary>
        [HttpPost("{categoryId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetCategory([FromBody] CatalogProductsCommandDto command,
            int categoryId)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);

            if (!await CheckCategoryAvailabilityAsync(category))
                return ApiResponseFactory.NotFound("The category is not available");

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewCategory"),
                    category.Name), category);

            //model
            var model = await _catalogModelFactory.PrepareCategoryModelAsync(category,
                command.FromDto<CatalogProductsCommand>());
            model.PictureModel = await _webApiCatalogModelFactory.PreparePictureModel(category);

            var customer = await _workContext.GetCurrentCustomerAsync();
            var fp = await _favoriteProductService.GetFavoriteProductsAsync(customer.Id);
            var fpIds = fp.Select(f => f.ProductId).ToList();
            
            
            var categoryModelDto = model.ToDto<CategoryModelDto>();
            foreach (var productDto in categoryModelDto.CatalogProductsModel.Products)
            {
                var product = await _productService.GetProductByIdAsync(productDto.Id);
                productDto.IsFavorite = fpIds.Contains(product.Id);
                productDto.VendorBrief = await _webApiCatalogModelFactory.PrepareVendorBriefInfoModelAsync(product.VendorId);
                productDto.ProductTags = await _webApiCatalogModelFactory.PrepareProductTagDtoAsync(productDto.Id);
                await _dispatcherService.PublishAsync("ProductDetailsModel", productDto);
                await _dispatcherService.PublishAsync(FrameworkDefaultValues.ProductDetailsEventTopic, productDto);
            }
            
            
            //sub category level
            var subCategoryLevel = command.SubCategoryLevel - 1;
            if (subCategoryLevel > 0)
            {
                foreach (var sub in categoryModelDto.SubCategories)
                {
                    sub.SubCategories = await _webApiCatalogModelFactory.PrepareSubCategoriesLevelDtoAsync(sub.Id, subCategoryLevel);
                }
            }
            
            //template
            var templateViewPath =
                await _catalogModelFactory.PrepareCategoryTemplateViewPathAsync(category.CategoryTemplateId);

            var response = new CategoryResponse
            {
                CategoryModelDto = categoryModelDto,
                TemplateViewPath = templateViewPath
            };
            return ApiResponseFactory.Success(response);
        }

        
        

        /// <summary>
        ///     Get the category products
        /// </summary>
        [HttpPost("{categoryId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GetCategoryProductsResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCategoryProducts([FromBody] CatalogProductsCommandDto command,
            int categoryId)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);

            if (!await CheckCategoryAvailabilityAsync(category))
                return ApiResponseFactory.NotFound("The category is not available");

            var model = await _catalogModelFactory.PrepareCategoryProductsModelAsync(category,
                command.FromDto<CatalogProductsCommand>());

            var catalogProducts = model.ToDto<CatalogProductsModelDto>();

            foreach (var dto in catalogProducts.Products)
            {
                await _dispatcherService.PublishAsync(FrameworkDefaultValues.ProductDetailsModelsEventTopic, dto);    
            }
            

            if (command.IncludeBanners && _entityAttachmentService != null)
            {
                foreach (var product in catalogProducts.Products)
                {
                    product.Banners = await _entityAttachmentService.GetAttachmentsAsync(nameof(product), product.Id);
                }
            }

            var customer = await _workContext.GetCurrentCustomerAsync();


            var fp = await _favoriteProductService.GetFavoriteProductsAsync(customer.Id);
            var fpIds = fp.Select(f => f.ProductId).ToList();
            
            
            foreach (var product in catalogProducts.Products)
            {
                product.IsFavorite = fpIds.Contains(product.Id);
            }

            var response = new GetCategoryProductsResponse
            {
                CatalogProductsModel = catalogProducts,
                TemplateViewPath = "_ProductsInGridOrLines"
            };
            return ApiResponseFactory.Success(response);
        }

        /// <summary>
        ///     Get catalog root (list of categories)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<CategorySimpleModelDto>), StatusCodes.Status200OK)]
        //[Authorize]
        [NonAction]
        public virtual async Task<IActionResult> GetCatalogRoot()
        {
            var model = await _catalogModelFactory.PrepareRootCategoriesAsync();
            var modelDto = model.Select(c => c.ToDto<CategorySimpleModelDto>()).ToList();

            return ApiResponseFactory.Success(modelDto);
        }

        /// <summary>
        ///     Get catalog sub categories
        /// </summary>
        /// <param names="id">Category identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IList<CategorySimpleModelDto>), StatusCodes.Status200OK)]
        //[Authorize]
        [NonAction]
        public virtual async Task<IActionResult> GetCatalogSubCategories(int id)
        {
            var model = await _catalogModelFactory.PrepareSubCategoriesAsync(id);
            var modelDto = model.Select(c => c.ToDto<CategorySimpleModelDto>()).ToList();

            return ApiResponseFactory.Success(modelDto);
        }

        #endregion

        #region Manufacturers

        /// <summary>
        ///     Get manufacturer
        /// </summary>
        [HttpPost("{manufacturerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ManufacturerResponse), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetManufacturer([FromBody] CatalogProductsCommandDto command,
            int manufacturerId)
        {
            var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);

            if (!await CheckManufacturerAvailabilityAsync(manufacturer))
                return ApiResponseFactory.NotFound("The manufacturer is not available");

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewManufacturer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewManufacturer"),
                    manufacturer.Name), manufacturer);

            //model
            var model = await _catalogModelFactory.PrepareManufacturerModelAsync(manufacturer,
                command.FromDto<CatalogProductsCommand>());

            //template
            var templateViewPath =
                await _catalogModelFactory.PrepareManufacturerTemplateViewPathAsync(manufacturer
                    .ManufacturerTemplateId);

            var response = new ManufacturerResponse
            {
                ManufacturerModel = model.ToDto<ManufacturerModelDto>(),
                TemplateViewPath = templateViewPath
            };
            return ApiResponseFactory.Success(response);
        }

        /// <summary>
        ///     Get manufacturer products
        /// </summary>
        [HttpPost("{manufacturerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GetManufacturerProductsResponse), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetManufacturerProducts([FromBody] CatalogProductsCommandDto command,
            int manufacturerId)
        {
            var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);

            if (!await CheckManufacturerAvailabilityAsync(manufacturer))
                return ApiResponseFactory.NotFound("The manufacturer is not available");

            var model = await _catalogModelFactory.PrepareManufacturerProductsModelAsync(manufacturer,
                command.FromDto<CatalogProductsCommand>());

            var response = new GetManufacturerProductsResponse
            {
                CatalogProductsModel = model.ToDto<CatalogProductsModelDto>(),
                TemplateViewPath = "_ProductsInGridOrLines"
            };

            return ApiResponseFactory.Success(response);
        }

        /// <summary>
        ///     Get all manufacturers
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ManufacturerModelDto>), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> ManufacturerAll()
        {
            var model = await _catalogModelFactory.PrepareManufacturerAllModelsAsync();
            var modelDto = model.Select(c => c.ToDto<ManufacturerModelDto>()).ToList();

            return ApiResponseFactory.Success(modelDto);
        }

        #endregion

        #region Vendors

        /// <summary>
        ///     Vendor
        /// </summary>
        [HttpPost("{vendorId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(VendorModelDto), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetVendor([FromBody] CatalogProductsCommandDto command, int vendorId)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

            if (!await CheckVendorAvailabilityAsync(vendor))
                return ApiResponseFactory.NotFound("The vendor is not available");

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            //model
            var model = await _catalogModelFactory.PrepareVendorModelAsync(vendor,
                command.FromDto<CatalogProductsCommand>());

            return ApiResponseFactory.Success(model.ToDto<VendorModelDto>());
        }

        /// <summary>
        ///     Get vendor products
        /// </summary>
        [HttpPost("{vendorId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GetVendorProductsResponse), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetVendorProducts([FromBody] CatalogProductsCommandDto command,
            int vendorId)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

            if (!await CheckVendorAvailabilityAsync(vendor))
                return ApiResponseFactory.NotFound("The vendor is not available");

            var model = await _catalogModelFactory.PrepareVendorProductsModelAsync(vendor,
                command.FromDto<CatalogProductsCommand>());

            var response = new GetVendorProductsResponse
            {
                CatalogProductsModel = model.ToDto<CatalogProductsModelDto>(),
                TemplateViewPath = "_ProductsInGridOrLines"
            };
            return ApiResponseFactory.Success(response);
        }

        /// <summary>
        ///     Get all vendors
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<VendorModelDto>), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> VendorAll()
        {
            //we don't allow viewing of vendors if "vendors" block is hidden
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_vendorSettings.VendorsBlockItemsToDisplay)} value = 0.");

            var model = await _catalogModelFactory.PrepareVendorAllModelsAsync();
            var modelDto = model.Select(c => c.ToDto<VendorModelDto>()).ToList();

            return ApiResponseFactory.Success(modelDto);
        }

        #endregion

        #region Product tags

        /// <summary>
        ///     Get products by tag
        /// </summary>
        [HttpPost("{productTagId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductsByTagModelDto), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetProductsByTag([FromBody] CatalogProductsCommandDto command,
            int productTagId)
        {
            var productTag = await _productTagService.GetProductTagByIdAsync(productTagId);
            if (productTag == null)
                return ApiResponseFactory.NotFound($"ProductTag by id={productTagId} not found.");

            var model = await _catalogModelFactory.PrepareProductsByTagModelAsync(productTag,
                command.FromDto<CatalogProductsCommand>());

            return ApiResponseFactory.Success(model.ToDto<ProductsByTagModelDto>());
        }

        /// <summary>
        ///     Get tag products
        /// </summary>
        [HttpPost("{productTagId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GetTagProductsResponse), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> GetTagProducts([FromBody] CatalogProductsCommandDto command,
            int productTagId)
        {
            var productTag = await _productTagService.GetProductTagByIdAsync(productTagId);
            if (productTag == null)
                return ApiResponseFactory.NotFound($"ProductTag by id={productTagId} not found.");

            var model = await _catalogModelFactory.PrepareTagProductsModelAsync(productTag,
                command.FromDto<CatalogProductsCommand>());
            var response = new GetTagProductsResponse
            {
                CatalogProductsModel = model.ToDto<CatalogProductsModelDto>(),
                TemplateViewPath = "_ProductsInGridOrLines"
            };

            return ApiResponseFactory.Success(response);
        }

        /// <summary>
        ///     Get all popular product tags
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PopularProductTagsModelDto), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> ProductTagsAll()
        {
            var model = await _catalogModelFactory.PreparePopularProductTagsModelAsync();

            return ApiResponseFactory.Success(model.ToDto<PopularProductTagsModelDto>());
        }

        #endregion

        #region Searching

        /// <summary>
        ///     Search
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(SearchModelDto), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> Search(SearchRequest request)
        {
            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(true),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            if (request.Model == null)
                request.Model = new SearchModel().ToDto<SearchModelDto>();

            var response = await _catalogModelFactory.PrepareSearchModelAsync(request.Model.FromDto<SearchModel>(),
                request.Command.FromDto<CatalogProductsCommand>());

            return ApiResponseFactory.Success(response.ToDto<SearchModelDto>());
        }

        /// <summary>
        ///     Search term auto complete
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<SearchTermAutoCompleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        //[Authorize]
        public virtual async Task<IActionResult> SearchTermAutoComplete([FromQuery] [Required] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return ApiResponseFactory.BadRequest(string.Empty);

            term = term.Trim();

            if (string.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return ApiResponseFactory.NotFound(
                    $"Term length is less {_catalogSettings.ProductSearchTermMinimumLength}.");

            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0
                ? _catalogSettings.ProductSearchAutoCompleteNumberOfProducts
                : 10;

            var products = await _productService.SearchProductsAsync(0,
                storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                keywords: term,
                languageId: (await _workContext.GetWorkingLanguageAsync()).Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);

            var showLinkToResultSearch = _catalogSettings.ShowLinkToAllResultInSearchAutoComplete &&
                                         products.TotalCount > productNumber;

            var models = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, false,
                _catalogSettings.ShowProductImagesInSearchAutoComplete,
                _mediaSettings.AutoCompleteSearchThumbPictureSize)).ToList();
            var result = (from p in models
                    select new SearchTermAutoCompleteResponse
                    {
                        Label = p.Name,
                        Producturl = Url.RouteUrl("Product", new { p.SeName }),
                        Productpictureurl = p.DefaultPictureModel.ImageUrl,
                        Showlinktoresultsearch = showLinkToResultSearch
                    })
                .ToList();
            return ApiResponseFactory.Success(result);
        }

        /// <summary>
        ///     Search products
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(SearchProductsResponse), StatusCodes.Status200OK)]
        //[Authorize]
        public virtual async Task<IActionResult> SearchProducts([FromBody] SearchRequest request)
        {
            if (request.Model == null)
                request.Model = new SearchModel().ToDto<SearchModelDto>();

            var model = await _catalogModelFactory.PrepareSearchProductsModelAsync(request.Model.FromDto<SearchModel>(),
                request.Command.FromDto<CatalogProductsCommand>());
            var response = new GetTagProductsResponse
            {
                CatalogProductsModel = model.ToDto<CatalogProductsModelDto>(),
                TemplateViewPath = "_ProductsInGridOrLines"
            };

            return ApiResponseFactory.Success(response);
        }

        #endregion

        #region Utilities

        private async Task<bool> CheckCategoryAvailabilityAsync(Category category)
        {
            var isAvailable = true;

            if (category == null || category.Deleted)
                return false;

            var notAvailable =
                //published?
                !category.Published ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(category) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(category);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) &&
                                 await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories);
            if (notAvailable && !hasAdminAccess)
                isAvailable = false;

            return isAvailable;
        }

        private async Task<bool> CheckManufacturerAvailabilityAsync(Manufacturer manufacturer)
        {
            var isAvailable = true;

            if (manufacturer == null || manufacturer.Deleted)
                return false;

            var notAvailable =
                //published?
                !manufacturer.Published ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(manufacturer) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(manufacturer);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) &&
                                 await _permissionService.AuthorizeAsync(StandardPermissionProvider
                                     .ManageManufacturers);
            if (notAvailable && !hasAdminAccess)
                isAvailable = false;

            return isAvailable;
        }

        private Task<bool> CheckVendorAvailabilityAsync(Vendor vendor)
        {
            var isAvailable = !(vendor == null || vendor.Deleted || !vendor.Active);

            return Task.FromResult(isAvailable);
        }

        #endregion

        #region Product

        [HttpGet]
        [ProducesResponseType(typeof(IList<ProductOverviewDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetHomepageProducts()
        {
            var products = await _productService.GetAllProductsDisplayedOnHomepageAsync();
        
            var productDtos = await _productModelFactory.PrepareProductOverviewModelsAsync(products, false, true, _mediaSettings.ProductThumbPictureSize);
        
            return ApiResponseFactory.Success(productDtos);
        }

        #endregion
    }
}

public class GetCategoriesByNamesRequest
{
    public List<string> Names { get; set; }
    public bool FeaturedProduct { get; set; }
    public int ProductCount { get; set; }
    public int SubCategoriesLevel { get; set; }
}