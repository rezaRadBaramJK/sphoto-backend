using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments;
using Nop.Plugin.Baramjk.Framework.Services.Products;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Product;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.FrontendApi.Factories
{
    public class WebApiProductModelFactory : IWebApiProductModelFactory
    {
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductDtoFactory _productDtoFactory;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IProductVisitService _productVisitService;
        private readonly ISearchProductService _searchProductService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IEntityAttachmentService _entityAttachmentService;
        private readonly IDispatcherService _dispatcherService;

        public WebApiProductModelFactory(ICategoryService categoryService, ICurrencyService currencyService,
            ICustomerService customerService, IDownloadService downloadService,
            ILocalizationService localizationService, IPermissionService permissionService,
            IPictureService pictureService, IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter, IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService, IProductService productService,
            IStaticCacheManager staticCacheManager, IStoreContext storeContext, ITaxService taxService,
            IWebHelper webHelper, IWorkContext workContext, MediaSettings mediaSettings, IProductVisitService productVisitService,
            ISearchProductService searchProductService, IProductModelFactory productModelFactory,
            IProductDtoFactory productDtoFactory, IDispatcherService dispatcherService)
        {
            _categoryService = categoryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _downloadService = downloadService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _taxService = taxService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _productVisitService = productVisitService;
            _searchProductService = searchProductService;
            _productModelFactory = productModelFactory;
            _productDtoFactory = productDtoFactory;
            _dispatcherService = dispatcherService;
            //!Fayyaz: DO NOT INJECT BY CONSTRUCTOR
            _entityAttachmentService = EngineContext.Current.Resolve<IEntityAttachmentService>();
        }

        public async Task<ProductSearchResultDto> SearchProductsAsync(GetProductsRequest command)
        {
            var withSpecification = command.PrepareSpecificationAttributes;
            var products = await FetchProductsAsync(command);
            var productsOverviewModels =
                await _productDtoFactory.PrepareProductOverviewAsync(products,
                    prepareSpecifications: withSpecification);

            var productSearchResultDto = new ProductSearchResultDto(productsOverviewModels.ToList(), products.PageIndex,
                products.PageSize, products.TotalCount);

            command.PageNumber = 1;
            command.PageSize = int.MaxValue;
            var allProducts = await FetchProductsAsync(command);
            var productIds = allProducts.Select(item => item.Id).ToArray();
            productSearchResultDto.Category = await GetCategoriesByProductIdsAsync(productIds);
            return productSearchResultDto;
        }

        public async Task<ProductListDto> PrepareProductsAsync(GetProductsRequest command)
        {
            var withSpecification = command.PrepareSpecificationAttributes;
            var products = await FetchProductsAsync(command);
            var productsOverviewModels = await _productDtoFactory.PrepareProductOverviewAsync(products,
                    prepareSpecifications: withSpecification, prepareAttributes: command.IncludeAttributes);

            await _dispatcherService.PublishAsync(FrameworkDefaultValues.ProductItemsEventTopic, productsOverviewModels);
            
            var productListDto = new ProductListDto(productsOverviewModels.ToList(), products.PageIndex,
                products.PageSize, products.TotalCount);

            return productListDto;
        }

        public async Task<ProductListDto> PrepareMyProductsAsync(int vendorId, int pageNumber = 1, int pageSize = 25)
        {
            var command = new GetProductsRequest
            {
                OrderBy = ProductSortingEnum.CreatedOn,
                PageNumber = pageNumber,
                PageSize = pageSize,
                VendorId = vendorId
            };

            var products = await FetchProductsAsync(command);
            var productOverviewDtos = await _productDtoFactory.PrepareProductOverviewAsync(products);
            var productListDto = new ProductListDto(productOverviewDtos, products.PageIndex, products.PageSize,
                products.TotalCount);

            return productListDto;
        }

        public async Task<List<ProductOverviewDto>> PrepareProductsAsync(int[] productIds)
        {
            var products = await _productService.GetProductsByIdsAsync(productIds);
            return await _productDtoFactory.PrepareProductOverviewAsync(products);
        }

        public async Task<ProductListDto> PrepareProductsMostVisitedAsync(int pageNumber = 0, int pageSize = 25)
        {
            var visitModels = await _productVisitService.GetProductsMostVisitedAsync(pageNumber, pageSize);
            var productIds = visitModels.Select(item => item.ProductId).ToArray();
            var products = await _productService.GetProductsByIdsAsync(productIds);
            var productList = (await _productDtoFactory.PrepareProductOverviewAsync(products)).ToList();

            var productOverviewModels = productList
                .Join(visitModels, item => item.Id
                    , item => item.ProductId, (productOverviewModel, visitModel) => new
                    {
                        product = productOverviewModel, visit = visitModel
                    })
                .OrderByDescending(item => item.visit.Count)
                .Select(item => item.product)
                .ToList();

            var productListDto = new ProductListDto(productOverviewModels, pageNumber + 1, pageSize, products.Count);
            return productListDto;
        }

        public async Task<ProductDetailsModelDto> PrepareProductDetailsModelAsync(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false)
        {
            var model = await _productModelFactory.PrepareProductDetailsModelAsync(product, updatecartitem,
                isAssociatedProduct);

            var productDetailsModelDto = model.ToFrameworkDto<ProductDetailsModelDto>();
            productDetailsModelDto.CreatedOnUtc = product.CreatedOnUtc;
            productDetailsModelDto.UpdatedOnUtc = product.UpdatedOnUtc;

            if (_entityAttachmentService != null)
                productDetailsModelDto.Banners = await _entityAttachmentService.GetAttachmentsAsync(
                    nameof(Product),
                    productDetailsModelDto.Id);

            foreach (var attributeMapping in productDetailsModelDto.ProductAttributes)
            {
                var attributeValues =
                    await _productAttributeService.GetProductAttributeValuesAsync(attributeMapping.Id);
                foreach (var valueModelDto in attributeMapping.Values)
                    valueModelDto.AssociatedProductId =
                        attributeValues.FirstOrDefault(i => i.Id == valueModelDto.Id)?.AssociatedProductId ?? 0;
            }

            return productDetailsModelDto;
        }
        
        private async Task<IPagedList<Product>> FetchProductsAsync(GetProductsRequest command)
        {
            return await _searchProductService.SearchProductsAsync(command);
        }

        protected async Task<IList<ProductDetailsModel.ProductAttributeModel>>
            PrepareProductAttributeModelsAsync(Product product, ShoppingCartItem updatecartitem)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new List<ProductDetailsModel.ProductAttributeModel>();

            var productAttributeMapping =
                await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in productAttributeMapping)
            {
                var productAttrubute =
                    await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);

                var attributeModel = new ProductDetailsModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Name),
                    Description = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Description),
                    TextPrompt = await _localizationService.GetLocalizedAsync(attribute, x => x.TextPrompt),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = updatecartitem != null
                        ? null
                        : await _localizationService.GetLocalizedAsync(attribute, x => x.DefaultValue),
                    HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml)
                };
                if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new ProductDetailsModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = attributeValue.IsPreSelected,
                            CustomerEntersQty = attributeValue.CustomerEntersQty,
                            Quantity = attributeValue.Quantity,
                            AssociatedProductId = attributeValue.AssociatedProductId
                        };
                        attributeModel.Values.Add(valueModel);

                        //display price if allowed
                        if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
                        {
                            var customer = updatecartitem?.CustomerId is null
                                ? await _workContext.GetCurrentCustomerAsync()
                                : await _customerService.GetCustomerByIdAsync(updatecartitem.CustomerId);

                            var attributeValuePriceAdjustment =
                                await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product,
                                    attributeValue, customer);
                            var (priceAdjustmentBase, _) =
                                await _taxService.GetProductPriceAsync(product, attributeValuePriceAdjustment);
                            var priceAdjustment =
                                await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceAdjustmentBase,
                                    await _workContext.GetWorkingCurrencyAsync());

                            if (attributeValue.PriceAdjustmentUsePercentage)
                            {
                                var priceAdjustmentStr = attributeValue.PriceAdjustment.ToString("G29");
                                if (attributeValue.PriceAdjustment > decimal.Zero)
                                    valueModel.PriceAdjustment = "+";
                                valueModel.PriceAdjustment += priceAdjustmentStr + "%";
                            }
                            else
                            {
                                if (priceAdjustmentBase > decimal.Zero)
                                    valueModel.PriceAdjustment = "+" +
                                                                 await _priceFormatter.FormatPriceAsync(priceAdjustment,
                                                                     false, false);
                                else if (priceAdjustmentBase < decimal.Zero)
                                    valueModel.PriceAdjustment = "-" +
                                                                 await _priceFormatter.FormatPriceAsync(
                                                                     -priceAdjustment, false, false);
                            }

                            valueModel.PriceAdjustmentValue = priceAdjustment;
                        }

                        //"image square" picture (with with "image squares" attribute type only)
                        if (attributeValue.ImageSquaresPictureId > 0)
                        {
                            var productAttributeImageSquarePictureCacheKey =
                                _staticCacheManager.PrepareKeyForDefaultCache(
                                    NopModelCacheDefaults.ProductAttributeImageSquarePictureModelKey
                                    , attributeValue.ImageSquaresPictureId,
                                    _webHelper.IsCurrentConnectionSecured(),
                                    await _storeContext.GetCurrentStoreAsync());
                            valueModel.ImageSquaresPictureModel = await _staticCacheManager.GetAsync(
                                productAttributeImageSquarePictureCacheKey, async () =>
                                {
                                    var imageSquaresPicture =
                                        await _pictureService.GetPictureByIdAsync(attributeValue.ImageSquaresPictureId);
                                    string fullSizeImageUrl, imageUrl;
                                    (imageUrl, imageSquaresPicture) =
                                        await _pictureService.GetPictureUrlAsync(imageSquaresPicture,
                                            _mediaSettings.ImageSquarePictureSize);
                                    (fullSizeImageUrl, imageSquaresPicture) =
                                        await _pictureService.GetPictureUrlAsync(imageSquaresPicture);

                                    if (imageSquaresPicture != null)
                                        return new PictureModel
                                        {
                                            FullSizeImageUrl = fullSizeImageUrl,
                                            ImageUrl = imageUrl
                                        };

                                    return new PictureModel();
                                });
                        }

                        //picture of a product attribute value
                        valueModel.PictureId = attributeValue.PictureId;
                    }
                }

                //set already selected attributeIds (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                        {
                            if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues =
                                    await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem
                                        .AttributesXml);
                                foreach (var attributeValue in selectedValues)
                                foreach (var item in attributeModel.Values)
                                    if (attributeValue.Id == item.Id)
                                    {
                                        item.IsPreSelected = true;

                                        //set customer entered quantity
                                        if (attributeValue.CustomerEntersQty)
                                            item.Quantity = attributeValue.Quantity;
                                    }
                            }
                        }

                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //values are already pre-set

                            //set customer entered quantity
                            if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                foreach (var attributeValue in (await _productAttributeParser
                                             .ParseProductAttributeValuesAsync(updatecartitem.AttributesXml))
                                         .Where(value => value.CustomerEntersQty))
                                {
                                    var item = attributeModel.Values.FirstOrDefault(value =>
                                        value.Id == attributeValue.Id);
                                    if (item != null)
                                        item.Quantity = attributeValue.Quantity;
                                }
                        }

                            break;
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        {
                            if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                            {
                                var enteredText =
                                    _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }

                            break;
                        case AttributeControlType.Datepicker:
                        {
                            //keep in mind my that the code below works only in the current culture
                            var selectedDateStr =
                                _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                            if (selectedDateStr.Any())
                                if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture,
                                        DateTimeStyles.None, out var selectedDate))
                                {
                                    //successfully parsed
                                    attributeModel.SelectedDay = selectedDate.Day;
                                    attributeModel.SelectedMonth = selectedDate.Month;
                                    attributeModel.SelectedYear = selectedDate.Year;
                                }
                        }

                            break;
                        case AttributeControlType.FileUpload:
                        {
                            if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                            {
                                var downloadGuidStr = _productAttributeParser
                                    .ParseValues(updatecartitem.AttributesXml, attribute.Id).FirstOrDefault();
                                Guid.TryParse(downloadGuidStr, out var downloadGuid);
                                var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                                if (download != null)
                                    attributeModel.DefaultValue = download.DownloadGuid.ToString();
                            }
                        }

                            break;
                    }

                model.Add(attributeModel);
            }

            return model;
        }

        private async Task<List<CategoryItemModel>> GetCategoriesByProductIdsAsync(int[] productIds)
        {
            var dictionary = await _categoryService.GetProductCategoryIdsAsync(productIds);
            var categoryIds = dictionary.SelectMany(item => item.Value).Distinct().ToArray();
            var categories = await _categoryService.GetCategoriesByIdsAsync(categoryIds);
            var categoryItemModels = await categories.SelectAwait(async item => new CategoryItemModel
            {
                Id = item.Id,
                Name = await _localizationService.GetLocalizedAsync(item, x => x.Name)
            }).ToListAsync();

            return categoryItemModels;
        }
    }
}