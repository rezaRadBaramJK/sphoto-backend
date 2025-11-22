using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Core.Html;
using Nop.Core.Rss;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Plugin.Baramjk.Framework.Services.Products;
using Nop.Plugin.Baramjk.FrontendApi.Dto;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Product;
using Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart;
using Nop.Plugin.Baramjk.FrontendApi.Factories;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Helpers;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Baramjk.FrontendApi.Services;
using Nop.Plugin.Baramjk.FrontendApi.Services.Models.AddProductServices;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class ProductController : BaseNopWebApiFrontendAllowAnonymousController
    {
        #region Ctor

        private readonly MediaSettings _mediaSettings;
        private readonly IPictureService _pictureService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDownloadService _downloadService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IReviewTypeService _reviewTypeService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly IWebApiProductModelFactory _webApiProductModelFactory;
        private readonly IProductVisitService _productVisitService;
        private readonly IRepository<ShoppingCartItem> _repositoryShoppingCartItem;
        private readonly IRepository<FavoriteProduct> _repositoryFavoriteProduct;
        private readonly ConditionAttributeService _conditionAttributeService;
        private readonly IProductTagService _productTagService;
        private readonly IAddProductService _addProductService;
        private readonly IProductDtoFactory _productDtoFactory;
        private readonly IDispatcherService _dispatcherService;
        private readonly FrontendProductService _frontendProductService;

        public ProductController(
            CatalogSettings catalogSettings,
            IAclService aclService,
            ICompareProductsService compareProductsService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDownloadService downloadService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPermissionService permissionService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            IReviewTypeService reviewTypeService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            ShoppingCartSettings shoppingCartSettings,
            ShippingSettings shippingSettings,
            IWebApiProductModelFactory webApiProductModelFactory,
            IProductVisitService productVisitService, 
            IRepository<ShoppingCartItem> repositoryShoppingCartItem,
            ConditionAttributeService conditionAttributeService,
            IPictureService pictureService,
            MediaSettings mediaSettings,
            IProductTagService productTagService,
            IAddProductService addProductService,
            IRepository<FavoriteProduct> repositoryFavoriteProduct, 
            IProductDtoFactory productDtoFactory,
            IDispatcherService dispatcherService,
            FrontendProductService frontendProductService)
        {
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _compareProductsService = compareProductsService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _downloadService = downloadService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _orderService = orderService;
            _permissionService = permissionService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _reviewTypeService = reviewTypeService;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _shippingSettings = shippingSettings;
            _webApiProductModelFactory = webApiProductModelFactory;
            _productVisitService = productVisitService;
            _repositoryShoppingCartItem = repositoryShoppingCartItem;
            _conditionAttributeService = conditionAttributeService;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
            _productTagService = productTagService;
            _addProductService = addProductService;
            _repositoryFavoriteProduct = repositoryFavoriteProduct;
            _productDtoFactory = productDtoFactory;
            _dispatcherService = dispatcherService;
            _frontendProductService = frontendProductService;
        }

        #endregion

        #region Recently viewed products

        /// <summary>
        ///     Get recently viewed products
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<ProductOverviewDto>), StatusCodes.Status200OK)]
        [Authorize]
        public virtual async Task<IActionResult> RecentlyViewedProducts()
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_catalogSettings.RecentlyViewedProductsEnabled)} is not enabled.");

            var products =
                await _recentlyViewedProductsService.GetRecentlyViewedProductsAsync(_catalogSettings
                    .RecentlyViewedProductsNumber);

            var overviewDtos = await _productDtoFactory.PrepareProductOverviewAsync(products);
            return ApiResponseFactory.Success(overviewDtos);
        }

        #endregion

        [HttpPost("")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductSearchResultDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Search([FromBody] GetProductsRequest command)
        {
            var model = await _webApiProductModelFactory.SearchProductsAsync(command);
            return ApiResponseFactory.Success(model);
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductListDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProducts([FromBody] GetProductsRequest command)
        {
            var model = await _webApiProductModelFactory.PrepareProductsAsync(command);

            return ApiResponseFactory.Success(model);
        }

        /// <summary>
        ///     Get Related Products
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(List<ProductOverviewDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetRelatedProducts([FromRoute] int id)
        {
            var productsOverviewModels = await GetRelatedProductsAsync(id);
            return ApiResponseFactory.Success(productsOverviewModels);
        }

        /// <summary>
        ///     Get new products (ordered by on create)
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductListDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetRecentlyProducts(int pageNumber = 1, int pageSize = 25)
        {
            var getProductsModelDto = new GetProductsRequest
            {
                OrderBy = ProductSortingEnum.CreatedOn,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var model = await _webApiProductModelFactory.PrepareProductsAsync(getProductsModelDto);

            return ApiResponseFactory.Success(model);
        }

        /// <summary>
        ///     Get products ordered by visit
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductListDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetMostVisited(int pageNumber = 1, int pageSize = 25)
        {
            var model = await _webApiProductModelFactory.PrepareProductsMostVisitedAsync(pageNumber, pageSize);
            return ApiResponseFactory.Success(model);
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(PriceRangeModel), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetPriceRange(int? manufacturerId = null, int? categoryId = null,
            int? vendorId = null)
        {
            var products = await _productService.SearchProductsAsync(
                0,
                1,
                manufacturerIds: manufacturerId == null ? null : new List<int> { manufacturerId.Value },
                categoryIds: categoryId == null ? null : new List<int> { categoryId.Value },
                vendorId: vendorId ?? 0,
                orderBy: ProductSortingEnum.PriceDesc);

            if (products.Any() == false)
                products = await _productService.SearchProductsAsync(
                    0,
                    1,
                    orderBy: ProductSortingEnum.PriceDesc);

            var priceRange = new PriceRangeModel
            {
                From = 0,
                To = products.FirstOrDefault()?.Price ?? int.MaxValue
            };

            return ApiResponseFactory.Success(priceRange);
        }

        /// <summary>
        ///     Get this vendor products
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public virtual async Task<IActionResult> GetMyProducts(int pageNumber = 1, int pageSize = 25)
        {
            var vendor = await _workContext.GetCurrentVendorAsync();
            if (vendor == null)
                return ApiResponseFactory.BadRequest("Vendor not found");

            var model = await _webApiProductModelFactory.PrepareMyProductsAsync(vendor.Id, pageNumber, pageSize);
            return ApiResponseFactory.Success(model);
        }

        [HttpDelete("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public virtual async Task<IActionResult> DeleteProduct([FromRoute] int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return ApiResponseFactory.NotFound($"Product Id={productId} not found");

            var hasPermission = await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts);
            if (hasPermission == false)
            {
                var vendor = await _workContext.GetCurrentVendorAsync();
                if (product.VendorId != vendor?.Id)
                    return ApiResponseFactory.BadRequest("You do not have permission to delete the product");
            }

            await _productService.DeleteProductAsync(product);

            return ApiResponseFactory.Success();
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(List<ConditionAttribute>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public virtual async Task<IActionResult> GetConditionAttributeAsync(int productId)
        {
            var attributes = await _conditionAttributeService.GetConditionAttributeAsync(productId);
            return ApiResponseFactory.Success(attributes);
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IEnumerable<ProductOverviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public virtual async Task<IActionResult> GetProductByIds([FromQuery] string productIds)
        {
            var productIdList = productIds.Split(",").Select(item => int.Parse(item)).ToArray();
            var model = await _webApiProductModelFactory.PrepareProductsAsync(productIdList);
            return ApiResponseFactory.Success(model);
        }

        [HttpGet]
        public virtual async Task<IActionResult> TagList([FromQuery] string tagName = null)
        {
            var productTags = await _productTagService.GetAllProductTagsAsync(tagName);
            return ApiResponseFactory.Success(productTags);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProductDataModel model)
        {
            var addProductResponse = await _addProductService.AddProductAsync(model);
            if (string.IsNullOrEmpty(addProductResponse.ErrorMessage) == false)
                return ApiResponseFactory.BadRequest<AddProductResponse>(null, addProductResponse.ErrorMessage);

            return ApiResponseFactory.Create(addProductResponse);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> EditProduct(int id, [FromBody] ProductDataModel model)
        {
            var addProductResponse = await _addProductService.EditProductAsync(id, model);
            if (string.IsNullOrEmpty(addProductResponse.ErrorMessage) == false)
                return ApiResponseFactory.BadRequest<AddProductResponse>(null, addProductResponse.ErrorMessage);

            return ApiResponseFactory.Create(addProductResponse);
        }

        private async Task<List<ProductOverviewDto>> GetCrossSellProducts(int productId)
        {
            var products = await _frontendProductService.GetCrossSellProductsAsync(productId);
            return await _productDtoFactory.PrepareProductOverviewAsync(products);
        }


        private async Task<List<ProductOverviewDto>> GetRelatedProductsAsync(int id)
        {
            var relatedProducts = await _productService.GetRelatedProductsByProductId1Async(id);
            var productIds = relatedProducts.Select(item => item.ProductId2).ToArray();
            var products = await _productService.GetProductsByIdsAsync(productIds);
            var productsOverviewModels = (await _productDtoFactory.PrepareProductOverviewAsync(products)).ToList();

            return productsOverviewModels;
        }

        protected virtual async Task<List<PictureModelDto>> PrepareProductDetailsPictureModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);

            //default picture size
            var defaultPictureSize = _mediaSettings.ProductDetailsPictureSize;

            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

            //all pictures
            var pictureModels = new List<PictureModelDto>();
            for (var i = 0; i < pictures.Count(); i++)
            {
                var picture = pictures[i];

                string imageUrl;
                string fullSizeImageUrl;
                string thumbImageUrl;

                (imageUrl, picture) =
                    await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture,
                    _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                var pictureModel = new PictureModelDto
                {
                    Id = picture.Id,
                    ImageUrl = imageUrl,
                    ThumbImageUrl = thumbImageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    Title = string.Format(
                        await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"),
                        productName),
                    AlternateText =
                        string.Format(
                            await _localizationService.GetResourceAsync(
                                "Media.Product.ImageAlternateTextFormat.Details"), productName)
                };
                //"title" attribute
                pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute)
                    ? picture.TitleAttribute
                    : string.Format(
                        await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"),
                        productName);
                //"alt" attribute
                pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute)
                    ? picture.AltAttribute
                    : string.Format(
                        await _localizationService.GetResourceAsync(
                            "Media.Product.ImageAlternateTextFormat.Details"), productName);

                pictureModels.Add(pictureModel);
            }

            return pictureModels;
        }


        #region Utilities

        protected virtual async ValueTask<IList<string>> ValidateProductReviewAvailabilityAsync(Product product,
            bool read = false)
        {
            var res = new List<string>();
            if(read==false)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                if (await _customerService.IsGuestAsync(customer) &&
                    !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                    res.Add(await _localizationService.GetResourceAsync("Reviews.OnlyRegisteredUsersCanWriteReviews"));
            }
            
            if (!_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing)
                return res;

            var hasCompletedOrders = product.ProductType == ProductType.SimpleProduct
                ? await HasCompletedOrdersAsync(product)
                : await (await _productService.GetAssociatedProductsAsync(product.Id)).AnyAwaitAsync(
                    HasCompletedOrdersAsync);

            if (!hasCompletedOrders)
                res.Add(await _localizationService.GetResourceAsync(
                    "Reviews.ProductReviewPossibleOnlyAfterPurchasing"));

            return res;
        }

        protected virtual async ValueTask<bool> HasCompletedOrdersAsync(Product product)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            return (await _orderService.SearchOrdersAsync(customerId: customer.Id,
                productId: product.Id,
                osIds: new List<int> { (int)OrderStatus.Complete },
                pageSize: 1)).Any();
        }

        /// <summary>
        ///     Parse a customer entered price of the product
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <returns>
        ///     A task that represents the asynchronous operation
        ///     The task result contains the customer entered price of the product
        /// </returns>
        protected virtual async Task<decimal> ParseCustomerEnteredPriceAsync(Product product,
            IDictionary<string, string> form)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var customerEnteredPriceConverted = decimal.Zero;
            if (product.CustomerEntersPrice)
                foreach (var formKey in form.Keys)
                    if (formKey.Equals($"addtocart_{product.Id}.CustomerEnteredPrice",
                            StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (decimal.TryParse(form[formKey], out var customerEnteredPrice))
                            customerEnteredPriceConverted =
                                await _currencyService.ConvertToPrimaryStoreCurrencyAsync(customerEnteredPrice,
                                    await _workContext.GetWorkingCurrencyAsync());
                        break;
                    }

            return customerEnteredPriceConverted;
        }

        /// <summary>
        ///     Parse a entered quantity of the product
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <returns>Customer entered price of the product</returns>
        protected virtual int ParseEnteredQuantity(Product product, IDictionary<string, string> form)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var quantity = 1;
            foreach (var formKey in form.Keys)
                if (formKey.Equals($"addtocart_{product.Id}.EnteredQuantity",
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    _ = int.TryParse(form[formKey], out quantity);
                    break;
                }

            return quantity;
        }

        /// <summary>
        ///     Get product attributes from the passed form
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form values</param>
        /// <param name="errors">Errors</param>
        /// <returns>
        ///     A task that represents the asynchronous operation
        ///     The task result contains the attributes in XML format
        /// </returns>
        protected virtual async Task<string> ParseProductAttributesAsync(Product product,
            IDictionary<string, string> form, List<string> errors)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            //product attributes
            var attributesXml = await GetProductAttributesXmlAsync(product, form, errors);

            //gift cards
            AddGiftCardsAttributesXml(product, form, ref attributesXml);

            return attributesXml;
        }

        /// <summary>
        ///     Gets product attributes in XML format
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <param name="errors">Errors</param>
        /// <returns>
        ///     A task that represents the asynchronous operation
        ///     The task result contains the attributes in XML format
        /// </returns>
        protected virtual async Task<string> GetProductAttributesXmlAsync(Product product,
            IDictionary<string, string> form, List<string> errors)
        {
            var attributesXml = string.Empty;
            var productAttributes =
                await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in productAttributes)
            {
                var controlId = $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                            {
                                //get quantity entered by customer
                                var quantity = 1;
                                var quantityStr =
                                    form[
                                        $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{selectedAttributeId}_qty"];
                                if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                    (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                    errors.Add(await _localizationService.GetResourceAsync(
                                        "Products.QuantityShouldBePositive"));

                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString(), quantity > 1 ? quantity : null);
                            }
                        }
                    }
                        break;
                    case AttributeControlType.Checkboxes:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            foreach (var item in ctrlAttributes
                                         .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                {
                                    //get quantity entered by customer
                                    var quantity = 1;
                                    var quantityStr =
                                        form[$"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{item}_qty"];
                                    if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                        (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                        errors.Add(await _localizationService.GetResourceAsync(
                                            "Products.QuantityShouldBePositive"));

                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString(), quantity > 1 ? quantity : null);
                                }
                            }
                    }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //load read-only (already server-side selected) values
                        var attributeValues =
                            await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                                     .Where(v => v.IsPreSelected)
                                     .Select(v => v.Id)
                                     .ToList())
                        {
                            //get quantity entered by customer
                            var quantity = 1;
                            var quantityStr =
                                form[
                                    $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{selectedAttributeId}_qty"];
                            if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                errors.Add(await _localizationService.GetResourceAsync(
                                    "Products.QuantityShouldBePositive"));

                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString(), quantity > 1 ? quantity : null);
                        }
                    }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.Trim();
                            attributesXml =
                                _productAttributeParser.AddProductAttribute(attributesXml, attribute, enteredText);
                        }
                    }
                        break;
                    case AttributeControlType.Datepicker:
                    {
                        var day = form[controlId + "_day"];
                        var month = form[controlId + "_month"];
                        var year = form[controlId + "_year"];
                        DateTime? selectedDate = null;
                        try
                        {
                            selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                        }
                        catch
                        {
                            // ignored
                        }

                        if (selectedDate.HasValue)
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml, attribute,
                                selectedDate.Value.ToString("D"));
                    }
                        break;
                    case AttributeControlType.FileUpload:
                    {
                        if (Guid.TryParse(form[controlId], out var downloadGuid))
                        {
                            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                            if (download != null)
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, download.DownloadGuid.ToString());
                        }
                    }
                        break;
                }
            }

            //validate conditional attributes (if specified)
            foreach (var attribute in productAttributes)
            {
                var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
            }

            return attributesXml;
        }

        /// <summary>
        ///     Adds gift cards attributes in XML format
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <param name="attributesXml">AttributeIds in XML format</param>
        protected virtual void AddGiftCardsAttributesXml(Product product, IDictionary<string, string> form,
            ref string attributesXml)
        {
            if (!product.IsGiftCard)
                return;

            var recipientName = "";
            var recipientEmail = "";
            var senderName = "";
            var senderEmail = "";
            var giftCardMessage = "";
            foreach (var formKey in form.Keys)
            {
                if (formKey.Equals($"giftcard_{product.Id}.RecipientName", StringComparison.InvariantCultureIgnoreCase))
                {
                    recipientName = form[formKey];
                    continue;
                }

                if (formKey.Equals($"giftcard_{product.Id}.RecipientEmail",
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    recipientEmail = form[formKey];
                    continue;
                }

                if (formKey.Equals($"giftcard_{product.Id}.SenderName", StringComparison.InvariantCultureIgnoreCase))
                {
                    senderName = form[formKey];
                    continue;
                }

                if (formKey.Equals($"giftcard_{product.Id}.SenderEmail", StringComparison.InvariantCultureIgnoreCase))
                {
                    senderEmail = form[formKey];
                    continue;
                }

                if (formKey.Equals($"giftcard_{product.Id}.Message", StringComparison.InvariantCultureIgnoreCase))
                    giftCardMessage = form[formKey];
            }

            attributesXml = _productAttributeParser.AddGiftCardAttribute(attributesXml, recipientName, recipientEmail,
                senderName, senderEmail, giftCardMessage);
        }

        /// <summary>
        ///     Parse product rental dates on the product details page
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        protected virtual void ParseRentalDates(Product product, IDictionary<string, string> form,
            out DateTime? startDate, out DateTime? endDate)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            startDate = null;
            endDate = null;

            if (product.IsRental)
            {
                var startControlId = $"rental_start_date_{product.Id}";
                var endControlId = $"rental_end_date_{product.Id}";
                var ctrlStartDate = form[startControlId];
                var ctrlEndDate = form[endControlId];
                try
                {
                    //currently we support only this format (as in the \Views\Product\_RentalInfo.cshtml file)
                    const string datePickerFormat = "d";
                    startDate = DateTime.ParseExact(ctrlStartDate, datePickerFormat, CultureInfo.InvariantCulture);
                    endDate = DateTime.ParseExact(ctrlEndDate, datePickerFormat, CultureInfo.InvariantCulture);
                }
                catch
                {
                    // ignored
                }
            }
        }

        #endregion

        #region Product details page

        /// <summary>
        ///     Get the product details
        /// </summary>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductDetails(
            int productId, 
            [FromQuery] int updateCartItemId = 0,
            bool getFavoriteCount = false,
            bool getRelatedProducts = false,
            bool getCrossSellProducts = false)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted)
                return ApiResponseFactory.NotFound($"No product found with the specified id={productId}");

            var notAvailable =
                //published?
                (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(product) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(product) ||
                //availability dates
                !_productService.ProductIsAvailable(product);
            //Check whether the current user has a "Manage products" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) &&
                                 await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts);
            if (notAvailable && !hasAdminAccess)
                return ApiResponseFactory.BadRequest();

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return ApiResponseFactory.NotFound(
                        $"Not found parentGroupedProduct by id={product.ParentGroupedProductId}");

                return ApiResponseFactory.NotFound("Product is not visible individually.");
            }

            //update existing shopping cart or wishlist  item?
            ShoppingCartItem updateCartItem = null;
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (_shoppingCartSettings.AllowCartItemEditing && updateCartItemId > 0)
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer,
                    storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
                updateCartItem = cart.FirstOrDefault(x => x.Id == updateCartItemId);
                //not found?
                if (updateCartItem == null)
                    return ApiResponseFactory.NotFound("The requested shopping cart is not found.");
                //is it this product?
                if (product.Id != updateCartItem.ProductId)
                    return ApiResponseFactory.BadRequest("The product does not match the requested.");
            }

            //save as recently viewed
            await _recentlyViewedProductsService.AddProductToRecentlyViewedListAsync(product.Id);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewProduct"),
                    product.Name), product);

            var productTemplateViewPath = await _productModelFactory.PrepareProductTemplateViewPathAsync(product);
            var modelDto = await _webApiProductModelFactory.PrepareProductDetailsModelAsync(product, updateCartItem);
            modelDto.PictureModels = await PrepareProductDetailsPictureModelAsync(product);
            modelDto.OrderMinimumQuantity = product.OrderMinimumQuantity;
            modelDto.OrderMaximumQuantity = product.OrderMaximumQuantity;
            modelDto.StockQuantity = product.StockQuantity;
            modelDto.ManageInventoryMethodId = product.ManageInventoryMethodId;
            modelDto.ProductCost = product.ProductCost;
            modelDto.Published = product.Published;

            if (getFavoriteCount)
            {
                modelDto.WishlistCount = await _repositoryShoppingCartItem.Table
                    .Where(item => item.ProductId == productId && item.ShoppingCartTypeId == 2)
                    .CountAsync();

                modelDto.FavoriteCount = await _repositoryFavoriteProduct.Table
                    .Where(item => item.ProductId == productId)
                    .CountAsync();

                modelDto.IsInWishlist = await _repositoryShoppingCartItem.Table
                    .AnyAsync(item =>
                        item.ProductId == productId && item.ShoppingCartTypeId == 2 && item.CustomerId == customer.Id);

                modelDto.IsFavorite = await _repositoryFavoriteProduct.Table
                    .AnyAsync(item => item.ProductId == productId && item.CustomerId == customer.Id);
            }

            if (getRelatedProducts)
                modelDto.CustomProperties["RelatedProducts"] = await GetRelatedProductsAsync(productId);

            if (getCrossSellProducts)
                modelDto.CustomProperties["CrossSellProducts"] = await GetCrossSellProducts(productId);

            await _dispatcherService.PublishAsync("ProductDetailsModel", modelDto);

            var response = new ProductDetailsResponse
            {
                ProductDetailsModel = modelDto,
                ProductTemplateViewPath = productTemplateViewPath
            };

            await _productVisitService.IncreaseVisitAsync(productId);

            return ApiResponseFactory.Success(response);
        }


        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductDetailsBySku([FromQuery] string sku)
        {
            var product = await _productService.GetProductBySkuAsync(sku);
            if (product == null || product.Deleted)
                return ApiResponseFactory.NotFound($"No product found with the specified sku={sku}");

            return await GetProductDetails(product.Id);
        }
        
        
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductDetailsBySeName(
            [FromQuery] string seName,
            [FromQuery] bool getFavoriteCount = false,
            [FromQuery] bool getRelatedProducts = false,
            [FromQuery] bool getCrossSellProducts = false)
        {
            var product = await _frontendProductService.GetProductBySeNameAsync(seName);
            if (product == null || product.Deleted)
                return ApiResponseFactory.NotFound($"No product found with the specified seName={seName}");

            return await GetProductDetails(product.Id, 0, getFavoriteCount, getRelatedProducts, getCrossSellProducts);
        }
        
        [HttpGet]
        public virtual async Task<IActionResult> GetProductsSeNamesAsync()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var seNames = await _frontendProductService.GetProductsSeNamesAsync(language);
            return ApiResponseFactory.Success(seNames);
        }


        /// <summary>
        ///     Get the estimate shipping
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(EstimateShippingResultModelDto), StatusCodes.Status200OK)]
        [Authorize]
        public virtual async Task<IActionResult> EstimateShipping(
            [FromBody] BaseModelDtoRequest<ProductEstimateShippingModelDto> request)
        {
            var errors = new List<string>();

            var model = request.Model.FromDto<ProductDetailsModel.ProductEstimateShippingModel>();

            if (model == null)
                model = new ProductDetailsModel.ProductEstimateShippingModel();

            if (!_shippingSettings.EstimateShippingCityNameEnabled && string.IsNullOrEmpty(model.ZipPostalCode))
                errors.Add(await _localizationService.GetResourceAsync(
                    "Shipping.EstimateShipping.ZipPostalCode.Required"));

            if (_shippingSettings.EstimateShippingCityNameEnabled && string.IsNullOrEmpty(model.City))
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.City.Required"));

            if (model.CountryId == null || model.CountryId == 0)
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.Country.Required"));

            if (errors.Count > 0)
                return ApiResponseFactory.Success(new EstimateShippingResultModelDto
                {
                    Success = false,
                    Errors = errors
                });

            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product == null || product.Deleted)
            {
                errors.Add(await _localizationService.GetResourceAsync(
                    "Shipping.EstimateShippingPopUp.Product.IsNotFound"));
                return ApiResponseFactory.Success(new EstimateShippingResultModelDto
                {
                    Success = false,
                    Errors = errors
                });
            }

            var wrappedProduct = new ShoppingCartItem
            {
                StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                ShoppingCartType = ShoppingCartType.ShoppingCart,
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                ProductId = product.Id,
                CreatedOnUtc = DateTime.UtcNow
            };

            var addToCartWarnings = new List<string>();
            //customer entered price
            wrappedProduct.CustomerEnteredPrice = await ParseCustomerEnteredPriceAsync(product, request.Form);

            //entered quantity
            wrappedProduct.Quantity = ParseEnteredQuantity(product, request.Form);

            //product and gift card attributes
            wrappedProduct.AttributesXml = await ParseProductAttributesAsync(product, request.Form, addToCartWarnings);

            //rental attributes
            ParseRentalDates(product, request.Form, out var rentalStartDate, out var rentalEndDate);
            wrappedProduct.RentalStartDateUtc = rentalStartDate;
            wrappedProduct.RentalEndDateUtc = rentalEndDate;

            var result =
                await _shoppingCartModelFactory.PrepareEstimateShippingResultModelAsync(new[] { wrappedProduct }, model,
                    false);

            return ApiResponseFactory.Success(result.ToDto<EstimateShippingResultModelDto>());
        }

        /// <summary>
        ///     Get product combinations
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<ProductCombinationModelDto>), StatusCodes.Status200OK)]
        [Authorize]
        public virtual async Task<IActionResult> GetProductCombinations(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return ApiResponseFactory.NotFound($"Product by id={productId} not found.");

            var model = await _productModelFactory.PrepareProductCombinationModelsAsync(product);
            var modelDto = model.Select(p => p.ToDto<ProductCombinationModelDto>()).ToList();
            return ApiResponseFactory.Success(modelDto);
        }

        #endregion

        #region New (recently added) products page

        /// <summary>
        ///     Get mark As new products (Not ordered by on create)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<ProductOverviewDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> NewProducts()
        {
            if (!_catalogSettings.NewProductsEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_catalogSettings.NewProductsEnabled)} is not enabled.");

            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var products = await _productService.GetProductsMarkedAsNewAsync(storeId);
            var overviewDtos = await _productDtoFactory.PrepareProductOverviewAsync(products);

            return ApiResponseFactory.Success(overviewDtos);
        }

        /// <summary>
        ///     Get mark As new products RSS(Not ordered by on create)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Authorize]
        public virtual async Task<IActionResult> NewProductsRss()
        {
            var feed = new RssFeed(
                $"{await _localizationService.GetLocalizedAsync(await _storeContext.GetCurrentStoreAsync(), x => x.Name)}: New products",
                "Information about products",
                new Uri(_webHelper.GetStoreLocation()),
                DateTime.UtcNow);

            if (!_catalogSettings.NewProductsEnabled)
                return ApiResponseFactory.Success(feed.GetContent());

            var items = new List<RssItem>();

            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var products = await _productService.GetProductsMarkedAsNewAsync(storeId);

            foreach (var product in products)
            {
                var productUrl = Url.RouteUrl("Product",
                    new { SeName = await _urlRecordService.GetSeNameAsync(product) },
                    _webHelper.GetCurrentRequestProtocol());
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
                var productDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription);
                var item = new RssItem(productName, productDescription, new Uri(productUrl),
                    $"urn:store:{(await _storeContext.GetCurrentStoreAsync()).Id}:newProducts:product:{product.Id}",
                    product.CreatedOnUtc);
                items.Add(item);
                //uncomment below if you want to add RSS enclosure for pictures
                //var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                //if (picture != null)
                //{
                //    var imageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductDetailsPictureSize);
                //    item.ElementExtensions.Add(new XElement("enclosure", new XAttribute("type", "image/jpeg"), new XAttribute("url", imageUrl), new XAttribute("length", picture.PictureBinary.Length)));
                //}
            }

            feed.Items = items;
            return ApiResponseFactory.Success(feed.GetContent());
        }

        #endregion

        #region Product reviews

        /// <summary>
        ///     Get product reviews
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductReviewsModelDto), StatusCodes.Status200OK)]
        [Authorize]
        public virtual async Task<IActionResult> ProductReviews(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return ApiResponseFactory.NotFound(
                    $"Product id={productId} not found or does not meet the required criteria.");

            var model = new ProductReviewsModel();
            model = await _productModelFactory.PrepareProductReviewsModelAsync(model, product);

            var resValidate = await ValidateProductReviewAvailabilityAsync(product, true);
            if (resValidate.Any())
                return ApiResponseFactory.BadRequest(resValidate);

            //default value
            model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;

            //default value for all additional review types
            if (model.ReviewTypeList.Count > 0)
                foreach (var additionalProductReview in model.AddAdditionalProductReviewList)
                    additionalProductReview.Rating = additionalProductReview.IsRequired
                        ? _catalogSettings.DefaultProductRatingValue
                        : 0;

            return ApiResponseFactory.Success(model.ToDto<ProductReviewsModelDto>());
        }

        /// <summary>
        ///     Add product reviews
        /// </summary>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductReviewsModelDto), StatusCodes.Status200OK)]
        [Authorize]
        public virtual async Task<IActionResult> ProductReviewsAdd([FromBody] ProductReviewsModelDto model,
            int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews ||
                !await _productService.CanAddReviewAsync(product.Id, (await _storeContext.GetCurrentStoreAsync()).Id))
                return ApiResponseFactory.NotFound(
                    $"Product id={productId} not found or does not meet the required criteria.");

            var resValidate = await ValidateProductReviewAvailabilityAsync(product);
            if (resValidate.Any())
                return ApiResponseFactory.BadRequest(resValidate);

            var productReviewsModel =
                await _productModelFactory.PrepareProductReviewsModelAsync(model.FromDto<ProductReviewsModel>(),
                    product);

            //save review
            var rating = model.AddProductReview.Rating;
            if (rating < 1 || rating > 5)
                rating = _catalogSettings.DefaultProductRatingValue;
            var isApproved = !_catalogSettings.ProductReviewsMustBeApproved;

            var productReview = new ProductReview
            {
                ProductId = product.Id,
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                Title = model.AddProductReview.Title,
                ReviewText = model.AddProductReview.ReviewText,
                Rating = rating,
                HelpfulYesTotal = 0,
                HelpfulNoTotal = 0,
                IsApproved = isApproved,
                CreatedOnUtc = DateTime.UtcNow,
                StoreId = (await _storeContext.GetCurrentStoreAsync()).Id
            };

            await _productService.InsertProductReviewAsync(productReview);

            //add product review and review type mapping                
            foreach (var additionalReview in model.AddAdditionalProductReviewList)
            {
                var additionalProductReview = new ProductReviewReviewTypeMapping
                {
                    ProductReviewId = productReview.Id,
                    ReviewTypeId = additionalReview.ReviewTypeId,
                    Rating = additionalReview.Rating
                };

                await _reviewTypeService.InsertProductReviewReviewTypeMappingsAsync(additionalProductReview);
            }

            //update product totals
            await _productService.UpdateProductReviewTotalsAsync(product);

            //notify store owner
            if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                await _workflowMessageService.SendProductReviewNotificationMessageAsync(productReview,
                    _localizationSettings.DefaultAdminLanguageId);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddProductReview",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddProductReview"),
                    product.Name), product);

            //raise event
            if (productReview.IsApproved)
                await _eventPublisher.PublishAsync(new ProductReviewApprovedEvent(productReview));

            productReviewsModel.AddProductReview.Title = null;
            productReviewsModel.AddProductReview.ReviewText = null;

            productReviewsModel.AddProductReview.SuccessfullyAdded = true;
            if (!isApproved)
                productReviewsModel.AddProductReview.Result =
                    await _localizationService.GetResourceAsync("Reviews.SeeAfterApproving");
            else
                productReviewsModel.AddProductReview.Result =
                    await _localizationService.GetResourceAsync("Reviews.SuccessfullyAdded");

            return ApiResponseFactory.Success(productReviewsModel.ToDto<ProductReviewsModelDto>());
        }

        /// <summary>
        ///     Set product review helpfulness
        /// </summary>
        /// <param name="productReviewId">Product review identifier</param>
        /// <param name="washelpful">Indicator if the review was helpful</param>
        [HttpPost("{productReviewId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SetProductReviewHelpfulnessResponse), StatusCodes.Status200OK)]
        [Authorize]
        public virtual async Task<IActionResult> SetProductReviewHelpfulness(int productReviewId,
            [FromQuery] [Required] bool washelpful)
        {
            var productReview = await _productService.GetProductReviewByIdAsync(productReviewId);
            if (productReview == null)
                return ApiResponseFactory.NotFound("No product review found with the specified id");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                return ApiResponseFactory.Success(new SetProductReviewHelpfulnessResponse
                {
                    Result = await _localizationService.GetResourceAsync("Reviews.Helpfulness.OnlyRegistered"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });

            //customers aren't allowed to vote for their own reviews
            if (productReview.CustomerId == (await _workContext.GetCurrentCustomerAsync()).Id)
                return ApiResponseFactory.Success(new SetProductReviewHelpfulnessResponse
                {
                    Result = await _localizationService.GetResourceAsync("Reviews.Helpfulness.YourOwnReview"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });

            await _productService.SetProductReviewHelpfulnessAsync(productReview, washelpful);

            //new totals
            await _productService.UpdateProductReviewHelpfulnessTotalsAsync(productReview);

            return ApiResponseFactory.Success(new SetProductReviewHelpfulnessResponse
            {
                Result = await _localizationService.GetResourceAsync("Reviews.Helpfulness.SuccessfullyVoted"),
                TotalYes = productReview.HelpfulYesTotal,
                TotalNo = productReview.HelpfulNoTotal
            });
        }

        /// <summary>
        ///     Customer product reviews for current customer
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerProductReviewsModelDto), StatusCodes.Status200OK)]
        [Authorize]
        public virtual async Task<IActionResult> CustomerProductReviews([FromQuery] int? pageNumber)
        {
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest("Customer is not registered.");

            if (!_catalogSettings.ShowProductReviewsTabOnAccountPage)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_catalogSettings.ShowProductReviewsTabOnAccountPage)} is not enabled.");

            var model = await _productModelFactory.PrepareCustomerProductReviewsModelAsync(pageNumber);

            return ApiResponseFactory.Success(model.ToDto<CustomerProductReviewsModelDto>());
        }

        #endregion

        #region Email a friend

        /// <summary>
        ///     ProductEmailAFriend
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductEmailAFriendModelDto), StatusCodes.Status200OK)]
        [Authorize]
        public virtual async Task<IActionResult> ProductEmailAFriend(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return ApiResponseFactory.NotFound(
                    $"Product id={productId} not found or does not meet the required criteria.");

            var model = new ProductEmailAFriendModel();
            model = await _productModelFactory.PrepareProductEmailAFriendModelAsync(model, product, false);
            return ApiResponseFactory.Success(model.ToDto<ProductEmailAFriendModelDto>());
        }

        /// <summary>
        ///     Send the product email a friend
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductEmailAFriendModelDto), StatusCodes.Status200OK)]
        [Authorize]
        public virtual async Task<IActionResult> ProductEmailAFriendSend([FromBody] ProductEmailAFriendModelDto model)
        {
            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return ApiResponseFactory.NotFound(
                    $"Product id={model.ProductId} not found or does not meet the required criteria.");

            //check whether the current customer is guest and ia allowed to email a friend
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_catalogSettings.AllowAnonymousUsersToEmailAFriend)
                return ApiResponseFactory.BadRequest(
                    await _localizationService.GetResourceAsync("Products.EmailAFriend.OnlyRegisteredUsers"));

            var productEmailAFriendModel =
                await _productModelFactory.PrepareProductEmailAFriendModelAsync(
                    model.FromDto<ProductEmailAFriendModel>(), product, true);

            //email
            await _workflowMessageService.SendProductEmailAFriendMessageAsync(
                await _workContext.GetCurrentCustomerAsync(),
                (await _workContext.GetWorkingLanguageAsync()).Id, product,
                model.YourEmailAddress, model.FriendEmail,
                HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false));

            productEmailAFriendModel.SuccessfullySent = true;
            productEmailAFriendModel.Result =
                await _localizationService.GetResourceAsync("Products.EmailAFriend.SuccessfullySent");

            return ApiResponseFactory.Success(productEmailAFriendModel.ToDto<ProductEmailAFriendModelDto>());
        }

        #endregion

        #region Comparing products

        /// <summary>
        ///     Add product to compare list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(AddProductToCompareListResponse), StatusCodes.Status200OK)]
        [Authorize]
        public virtual async Task<IActionResult> AddProductToCompareList(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published)
                return ApiResponseFactory.Success(new AddProductToCompareListResponse
                {
                    Success = false,
                    Message = "No product found with the specified ID"
                });

            if (!_catalogSettings.CompareProductsEnabled)
                return ApiResponseFactory.Success(new AddProductToCompareListResponse
                {
                    Success = false,
                    Message = "Product comparison is disabled"
                });

            await _compareProductsService.AddProductToCompareListAsync(productId);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddToCompareList",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToCompareList"),
                    product.Name), product);

            return ApiResponseFactory.Success(new AddProductToCompareListResponse
            {
                Success = true,
                Message = string.Format(
                    await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToCompareList.Link"),
                    Url.RouteUrl("CompareProducts"))
                //use the code below (commented) if you want a customer to be automatically redirected to the compare products page
                //redirect = Url.RouteUrl("CompareProducts"),
            });
        }

        /// <summary>
        ///     Remove product from compare list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public virtual async Task<IActionResult> RemoveProductFromCompareList(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return ApiResponseFactory.NotFound(
                    $"Product id={productId} not found or does not meet the required criteria.");

            if (!_catalogSettings.CompareProductsEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_catalogSettings.CompareProductsEnabled)} is not enabled.");

            await _compareProductsService.RemoveProductFromCompareListAsync(productId);

            return ApiResponseFactory.Success();
        }

        /// <summary>
        ///     Compare products
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CompareProductsModelDto), StatusCodes.Status200OK)]
        [Authorize]
        public virtual async Task<IActionResult> CompareProducts()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_catalogSettings.CompareProductsEnabled)} is not enabled.");

            var model = new CompareProductsModel
            {
                IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
                IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts
            };

            var products = await (await _compareProductsService.GetComparedProductsAsync())
                //ACL and store mapping
                .WhereAwait(async p =>
                    await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
                //availability dates
                .Where(p => _productService.ProductIsAvailable(p)).ToListAsync();

            //prepare model
            (await _productModelFactory.PrepareProductOverviewModelsAsync(products,
                    prepareSpecificationAttributes: true))
                .ToList()
                .ForEach(model.Products.Add);

            return ApiResponseFactory.Success(model.ToDto<CompareProductsModelDto>());
        }

        /// <summary>
        ///     Clear compare products list
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual IActionResult ClearCompareList()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_catalogSettings.CompareProductsEnabled)} is not enabled.");

            _compareProductsService.ClearCompareProducts();

            return ApiResponseFactory.Success();
        }

        #endregion
    }
}