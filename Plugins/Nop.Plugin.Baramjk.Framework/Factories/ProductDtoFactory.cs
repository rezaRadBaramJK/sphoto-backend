using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Models.Categories;
using Nop.Plugin.Baramjk.Framework.Models.Pictures;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using Nop.Plugin.Baramjk.Framework.Models.Vendors;
using Nop.Plugin.Baramjk.Framework.Services.Products;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.Framework.Factories
{
    public class ProductDtoFactory : IProductDtoFactory
    {
        private readonly IRepository<AclRecord> _aclRecordRepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IProductVisitService _productVisitService;
        private readonly IRepository<FavoriteProduct> _repositoryFavoriteProduct;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IProductAttributeService _productAttributeService;
        
        public ProductDtoFactory(CatalogSettings catalogSettings, ICategoryService categoryService,
            ICurrencyService currencyService, ILocalizationService localizationService,
            IPermissionService permissionService, IPictureService pictureService,
            IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter,
            IProductService productService, IProductTagService productTagService,
            IStaticCacheManager staticCacheManager, IStoreContext storeContext, ITaxService taxService,
            IUrlRecordService urlRecordService, IWebHelper webHelper, IWorkContext workContext,
            MediaSettings mediaSettings, OrderSettings orderSettings, IProductVisitService productVisitService,
            IRepository<FavoriteProduct> repositoryFavoriteProduct, IRepository<AclRecord> aclRecordRepository,
            IShoppingCartService shoppingCartService,
            IRepository<ProductAttributeMapping> productAttributeMappingRepository,
            IRepository<Vendor> vendorRepository, 
            ISpecificationAttributeService specificationAttributeService, 
            IProductAttributeService productAttributeService)
        {
            _catalogSettings = catalogSettings;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _productTagService = productTagService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _taxService = taxService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _productVisitService = productVisitService;
            _repositoryFavoriteProduct = repositoryFavoriteProduct;
            _aclRecordRepository = aclRecordRepository;
            _shoppingCartService = shoppingCartService;
            _productAttributeMappingRepository = productAttributeMappingRepository;
            _vendorRepository = vendorRepository;
            _specificationAttributeService = specificationAttributeService;
            _productAttributeService = productAttributeService;
        }

        public async Task<List<ProductOverviewDto>> PrepareProductOverviewAsync(int[] productIds)
        {
            var products = await _productService.GetProductsByIdsAsync(productIds);
            return await PrepareProductOverviewAsync(products);
        }

        public async Task<List<ProductOverviewDto>> PrepareProductOverviewAsync(
            IEnumerable<Product> products, bool preparePrice = true, bool preparePicture = true,
            int? thumbSize = null, bool prepareSpecifications = true, bool prepareTag = true,
            bool prepareCategories = true, bool prepareAttributes = false)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var productIds = products.Select(item => item.Id).ToList();

            var customer = await _workContext.GetCurrentCustomerAsync();

            var mappings = await GetProductAttributeMappingsAsync(productIds);
            var shoppingCartItems = await _shoppingCartService.GetShoppingCartAsync(customer);

            var vendorIds = products.Select(item => item.VendorId).Distinct().ToList();
            var vendors = _vendorRepository.Table.Where(item => vendorIds.Contains(item.Id))
                .Select(item => new VendorItemDto { Id = item.Id, Name = item.Name })
                .ToDictionary(item => item.Id);

            var customerRoleIds = _aclRecordRepository.Table
                .Where(item => item.EntityName == "Product")
                .Where(item => productIds.Contains(item.EntityId))
                .Select(item => new
                {
                    item.EntityId,
                    item.CustomerRoleId
                }).ToList();

            var favoriteProducts = _repositoryFavoriteProduct.Table
                .Where(item => item.CustomerId == customer.Id)
                .Select(item => item.ProductId)
                .Distinct()
                .ToHashSet();

            var visitModels = await _productVisitService.GetProductsVisitAsync(productIds);

            var models = new List<ProductOverviewDto>();
            foreach (var product in products)
            {
                var model = new ProductOverviewDto
                {
                    Id = product.Id,
                    Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                    ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                    FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                    SeName = await _urlRecordService.GetSeNameAsync(product),
                    Sku = product.Sku,
                    ProductType = product.ProductType,
                    CreatedOnUtc = product.CreatedOnUtc,
                    AvailableStartDateTimeUtc = product.AvailableStartDateTimeUtc,
                    AvailableEndDateTimeUtc = product.AvailableEndDateTimeUtc,
                    IsOutOfStock = await GetProductIsSoldOutValue(product),
                    VendorId = product.VendorId,
                    MarkAsNew = product.MarkAsNew &&
                                (!product.MarkAsNewStartDateTimeUtc.HasValue ||
                                 product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                                (!product.MarkAsNewEndDateTimeUtc.HasValue ||
                                 product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow),
                    ReviewOverviewModel = await PrepareProductReviewOverviewModelAsync(product),
                    VisitCount = visitModels.FirstOrDefault(v => v.ProductId == product.Id)?.Count ?? 0,
                    
                };

                if (preparePrice)
                    model.ProductPrice = await PrepareProductOverviewPriceModelAsync(product);

                if (preparePicture)
                    model.DefaultPictureModel = await PrepareProductOverviewPictureModelAsync(product, thumbSize);

                if (prepareSpecifications)
                    model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);

                if (prepareTag)
                    model.ProductTags = await GetProductTagsAsync(product);

                if (prepareCategories)
                    model.Categories = await GetProductCategoriesAsync(product);

                if (product.VendorId > 0 && vendors.TryGetValue(product.VendorId, out var vendor))
                    model.Vendor = vendor;

                var attributeMappings = mappings.Where(m => m.ProductId == product.Id).ToList();
                model.HasAttribute = attributeMappings.Any();
                model.HasRequiredAttribute = attributeMappings.Any(item =>item.IsRequired);
                if(prepareAttributes)
                    model.ProductAttributes = await PrepareProductAttributesAsync(attributeMappings);

                model.IsInWishlist = shoppingCartItems.Any(item =>
                    item.ProductId == product.Id && item.ShoppingCartType == ShoppingCartType.Wishlist);

                model.IsInShoppingCart = shoppingCartItems.Any(item =>
                    item.ProductId == product.Id && item.ShoppingCartType == ShoppingCartType.ShoppingCart);

                model.IsFavorite = favoriteProducts.Contains(model.Id);

                model.CustomerRoleIds = customerRoleIds.Where(c => c.EntityId == model.Id)
                    .Select(item => item.CustomerRoleId)
                    .ToList();

                models.Add(model);
            }

            return models;
        }

        private async Task<bool> GetProductIsSoldOutValue(Product product)
        {
            switch (product.ManageInventoryMethod)
            {
                case ManageInventoryMethod.DontManageStock:
                case ManageInventoryMethod.ManageStock when product.StockQuantity > 0:
                    return false;
                case ManageInventoryMethod.ManageStock:
                    return true;
                case ManageInventoryMethod.ManageStockByAttributes:
                {
                    var productAttributeCombinations = 
                        await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
                    
                    return !productAttributeCombinations.Any(x => x.StockQuantity > 0);
                }
                default:
                    throw new ArgumentOutOfRangeException($"failed to set out of stock for {product.Id}");
            }
        }

        private async Task<IList<ProductDetailsAttributeModelDto>> PrepareProductAttributesAsync(
            List<ProductAttributeMapping> attributeMappings)
        {
            return await attributeMappings.SelectAwait(async mapping =>
            {
                var attribute = await _productAttributeService.GetProductAttributeByIdAsync(mapping.ProductAttributeId);
                var values = await _productAttributeService.GetProductAttributeValuesAsync(mapping.Id);
                return new ProductDetailsAttributeModelDto
                {
                    Id = mapping.Id,
                    ProductId = mapping.ProductId,
                    ProductAttributeId = mapping.ProductAttributeId,
                    Name = await _localizationService.GetLocalizedAsync(attribute, a => a.Name),
                    Description = await _localizationService.GetLocalizedAsync(attribute, a => a.Description),
                    TextPrompt = await _localizationService.GetLocalizedAsync(mapping, m => m.TextPrompt),
                    IsRequired = mapping.IsRequired,
                    DefaultValue = await _localizationService.GetLocalizedAsync(mapping, m => m.DefaultValue),
                    HasCondition = !string.IsNullOrEmpty(mapping.ConditionAttributeXml),
                    AllowedFileExtensions = 
                        string.IsNullOrEmpty(mapping.ValidationFileAllowedExtensions) 
                        ? new List<string>()  
                        : mapping.ValidationFileAllowedExtensions
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                    AttributeControlType = mapping.AttributeControlTypeId,
                    Values = await values.SelectAwait(async value => new ProductAttributeValueModelDto
                    {
                        Id = value.Id,
                        Name = await _localizationService.GetLocalizedAsync(value, v => v.Name),
                        ColorSquaresRgb = value.ColorSquaresRgb,
                        ImageSquaresPictureModel = await PrepareAttributeValuePictureAsync(value.ImageSquaresPictureId),
                        PriceAdjustment = await _priceFormatter.FormatPriceAsync(value.PriceAdjustment),
                        PriceAdjustmentUsePercentage = value.PriceAdjustmentUsePercentage,
                        PriceAdjustmentValue = value.PriceAdjustment,
                        IsPreSelected = value.IsPreSelected,
                        PictureId = value.PictureId,
                        CustomerEntersQty = value.CustomerEntersQty,
                        Quantity = value.Quantity,
                        AssociatedProductId = value.AssociatedProductId
                    }).ToListAsync()
                };
            }).ToListAsync();
        }

        private async Task<DefaultPictureDto> PrepareAttributeValuePictureAsync(int imageSquaresPictureId)
        {
            var productAttributeImageSquarePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                NopModelCacheDefaults.ProductAttributeImageSquarePictureModelKey
                , imageSquaresPictureId,
                _webHelper.IsCurrentConnectionSecured(),
                await _storeContext.GetCurrentStoreAsync());
            
            return await _staticCacheManager.GetAsync(productAttributeImageSquarePictureCacheKey, async () =>
            {
                var imageSquaresPicture = await _pictureService.GetPictureByIdAsync(imageSquaresPictureId);
                string fullSizeImageUrl, imageUrl;
                (imageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture, _mediaSettings.ImageSquarePictureSize);
                (fullSizeImageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture);

                if (imageSquaresPicture != null)
                {
                    return new DefaultPictureDto
                    {
                        FullSizeImageUrl = fullSizeImageUrl,
                        ImageUrl = imageUrl
                    };
                }
                return new DefaultPictureDto();
            });
        }

        public async Task<ProductBriefModel> PrepareProductBriefModelAsync(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return null;

            return await PrepareProductBriefModelAsync(product);
        }

        public async Task<ProductBriefModel> PrepareProductBriefModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new ProductBriefModel
            {
                Id = product.Id, Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                CreatedOnUtc = product.CreatedOnUtc,
                AvailableStartDateTimeUtc = product.AvailableStartDateTimeUtc,
                AvailableEndDateTimeUtc = product.AvailableEndDateTimeUtc,
                VendorId = product.VendorId,
                ProductPrice = await PrepareProductOverviewPriceModelAsync(product),
                PictureModel = await PrepareProductOverviewPictureModelAsync(product)
            };

            return model;
        }

        public async Task<ProductOverviewModel.ProductPriceModel> PrepareProductOverviewPriceModelAsync(
            Product product, bool forceRedirectionAfterAddingToCart = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var priceModel = new ProductOverviewModel.ProductPriceModel
            {
                ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart
            };

            switch (product.ProductType)
            {
                case ProductType.GroupedProduct:
                    //grouped product
                    await PrepareGroupedProductOverviewPriceModelAsync(product, priceModel);

                    break;
                case ProductType.SimpleProduct:
                default:
                    //simple product
                    await PrepareSimpleProductOverviewPriceModelAsync(product, priceModel);

                    break;
            }

            return priceModel;
        }

        public async Task<PictureModel> PrepareProductOverviewPictureModelAsync(Product product,
            int? productThumbPictureSize = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
            //If a size has been set in the view, we use it in priority
            var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            //prepare picture model
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                NopModelCacheDefaults.ProductDefaultPictureModelKey,
                product, pictureSize, true, await _workContext.GetWorkingLanguageAsync(),
                _webHelper.IsCurrentConnectionSecured(),
                await _storeContext.GetCurrentStoreAsync());

            var defaultPictureModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                string fullSizeImageUrl, imageUrl;
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

                var pictureModel = new PictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    //"title" attribute
                    Title = picture != null && !string.IsNullOrEmpty(picture.TitleAttribute)
                        ? picture.TitleAttribute
                        : string.Format(
                            await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"),
                            productName),
                    //"alt" attribute
                    AlternateText = picture != null && !string.IsNullOrEmpty(picture.AltAttribute)
                        ? picture.AltAttribute
                        : string.Format(
                            await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"),
                            productName)
                };

                return pictureModel;
            });

            return defaultPictureModel;
        }

        public async Task<IList<ProductTag>> GetProductTagsAsync(Product product)
        {
            var tags = await _productTagService.GetAllProductTagsByProductIdAsync(product.Id);
            return tags;
        }

        public async Task<List<CategoryItemDto>> GetProductCategoriesAsync(Product product)
        {
            var categoryItemModels = new List<CategoryItemDto>();
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
            foreach (var productCategory in productCategories)
            {
                var category = await _categoryService.GetCategoryByIdAsync(productCategory.CategoryId);
                var categoryItemModel = new CategoryItemDto
                {
                    Id = category.Id,
                    Name = category.Name
                };
                categoryItemModels.Add(categoryItemModel);
            }

            return categoryItemModels;
        }

        public async Task<CategoryItemDto> GetHighestOrderProductCategoryAsync(int productId)
        {
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(productId);
            if (!productCategories.Any())
            {
                return null;
            }

            var mainCategory = productCategories.OrderBy(x => x.DisplayOrder).First();
            var category = await _categoryService.GetCategoryByIdAsync(mainCategory.CategoryId);

            return new CategoryItemDto
            {
                Id = mainCategory.Id,
                Name = category.Name
            };
        }

        public virtual async Task<ProductSpecificationModel> PrepareProductSpecificationModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new ProductSpecificationModel();

            // Add non-grouped attributes first
            model.Groups.Add(new ProductSpecificationAttributeGroupModel
            {
                Attributes = await PrepareProductSpecificationAttributeModelAsync(product, null)
            });

            // Add grouped attributes
            var groups = await _specificationAttributeService.GetProductSpecificationAttributeGroupsAsync(product.Id);
            foreach (var group in groups)
                model.Groups.Add(new ProductSpecificationAttributeGroupModel
                {
                    Id = group.Id,
                    Name = await _localizationService.GetLocalizedAsync(group, x => x.Name),
                    Attributes = await PrepareProductSpecificationAttributeModelAsync(product, group)
                });

            return model;
        }

        private async Task<ProductReviewOverviewModel> PrepareProductReviewOverviewModelAsync(Product product)
        {
            ProductReviewOverviewModel productReview;

            if (_catalogSettings.ShowProductReviewsPerStore)
            {
                var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                    NopModelCacheDefaults.ProductReviewsModelKey, product, await _storeContext.GetCurrentStoreAsync());

                productReview = await _staticCacheManager.GetAsync(cacheKey, async () =>
                {
                    var productReviews = await _productService.GetAllProductReviewsAsync(productId: product.Id,
                        approved: true, storeId: (await _storeContext.GetCurrentStoreAsync()).Id);

                    return new ProductReviewOverviewModel
                    {
                        RatingSum = productReviews.Sum(pr => pr.Rating),
                        TotalReviews = productReviews.Count
                    };
                });
            }
            else
            {
                productReview = new ProductReviewOverviewModel
                {
                    RatingSum = product.ApprovedRatingSum,
                    TotalReviews = product.ApprovedTotalReviews
                };
            }

            if (productReview != null)
            {
                productReview.ProductId = product.Id;
                productReview.AllowCustomerReviews = product.AllowCustomerReviews;
                productReview.CanAddNewReview =
                    await _productService.CanAddReviewAsync(product.Id,
                        (await _storeContext.GetCurrentStoreAsync()).Id);
            }

            return productReview;
        }

        private async Task<List<ProductAttributeMapping>> GetProductAttributeMappingsAsync(List<int> productIds)
        {
            var query = from pam in _productAttributeMappingRepository.Table
                orderby pam.DisplayOrder, pam.Id
                where productIds.Contains(pam.ProductId)
                select pam;

            return await query.ToListAsync();
        }

        private async Task PrepareSimpleProductOverviewPriceModelAsync(Product product,
            ProductOverviewModel.ProductPriceModel priceModel)
        {
            //add to cart button
            priceModel.DisableBuyButton = product.DisableBuyButton ||
                                          !await _permissionService.AuthorizeAsync(StandardPermissionProvider
                                              .EnableShoppingCart) ||
                                          !await _permissionService.AuthorizeAsync(StandardPermissionProvider
                                              .DisplayPrices);

            //add to wishlist button
            priceModel.DisableWishlistButton = product.DisableWishlistButton ||
                                               !await _permissionService.AuthorizeAsync(StandardPermissionProvider
                                                   .EnableWishlist) ||
                                               !await _permissionService.AuthorizeAsync(StandardPermissionProvider
                                                   .DisplayPrices);
            //compare products
            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

            //rental
            priceModel.IsRental = product.IsRental;

            //pre-order
            if (product.AvailableForPreOrder)
            {
                priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                                  product.PreOrderAvailabilityStartDateTimeUtc.Value >=
                                                  DateTime.UtcNow;
                priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
            }

            //prices
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                if (product.CustomerEntersPrice)
                    return;

                if (product.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                     _workContext.OriginalCustomerIfImpersonated == null))
                {
                    //call for price
                    priceModel.OldPrice = null;
                    priceModel.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
                }
                else
                {
                    //prices
                    var (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount, _, _) =
                        await _priceCalculationService.GetFinalPriceAsync(product,
                            await _workContext.GetCurrentCustomerAsync());

                    if (product.HasTierPrices)
                    {
                        var (tierPriceMinPossiblePriceWithoutDiscount, tierPriceMinPossiblePriceWithDiscount, _, _) =
                            await _priceCalculationService.GetFinalPriceAsync(product,
                                await _workContext.GetCurrentCustomerAsync(), quantity: int.MaxValue);

                        //calculate price for the maximum quantity if we have tier prices, and choose minimal
                        minPossiblePriceWithoutDiscount = Math.Min(minPossiblePriceWithoutDiscount,
                            tierPriceMinPossiblePriceWithoutDiscount);
                        minPossiblePriceWithDiscount = Math.Min(minPossiblePriceWithDiscount,
                            tierPriceMinPossiblePriceWithDiscount);
                    }

                    var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
                    var (finalPriceWithoutDiscountBase, _) =
                        await _taxService.GetProductPriceAsync(product, minPossiblePriceWithoutDiscount);
                    var (finalPriceWithDiscountBase, _) =
                        await _taxService.GetProductPriceAsync(product, minPossiblePriceWithDiscount);

                    var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase,
                        await _workContext.GetWorkingCurrencyAsync());
                    var finalPriceWithoutDiscount =
                        await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithoutDiscountBase,
                            await _workContext.GetWorkingCurrencyAsync());
                    var finalPriceWithDiscount =
                        await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase,
                            await _workContext.GetWorkingCurrencyAsync());

                    //do we have tier prices configured?
                    var tierPrices = new List<TierPrice>();
                    if (product.HasTierPrices)
                        tierPrices.AddRange(await _productService.GetTierPricesAsync(product,
                            await _workContext.GetCurrentCustomerAsync(),
                            (await _storeContext.GetCurrentStoreAsync()).Id));

                    //When there is just one tier price (with  qty 1), there are no actual savings in the list.
                    var displayFromMessage =
                        tierPrices.Any() && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
                    if (displayFromMessage)
                    {
                        priceModel.OldPrice = null;
                        priceModel.Price =
                            string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"),
                                await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount));
                        priceModel.PriceValue = finalPriceWithDiscount;
                    }
                    else
                    {
                        var strikeThroughPrice = decimal.Zero;

                        if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                            strikeThroughPrice = oldPrice;

                        if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                            strikeThroughPrice = finalPriceWithoutDiscount;

                        if (strikeThroughPrice > decimal.Zero)
                            priceModel.OldPrice = await _priceFormatter.FormatPriceAsync(strikeThroughPrice);

                        priceModel.Price = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                        priceModel.PriceValue = finalPriceWithDiscount;
                    }

                    if (product.IsRental)
                    {
                        //rental product
                        priceModel.OldPrice =
                            await _priceFormatter.FormatRentalProductPeriodAsync(product, priceModel.OldPrice);
                        priceModel.Price =
                            await _priceFormatter.FormatRentalProductPeriodAsync(product, priceModel.Price);
                    }

                    //property for German market
                    //we display tax/shipping info only with "shipping enabled" for this product
                    //we also ensure this it's not free shipping
                    priceModel.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductBoxes &&
                                                        product.IsShipEnabled && !product.IsFreeShipping;

                    //PAngV default baseprice (used in Germany)
                    priceModel.BasePricePAngV =
                        await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscount);
                }
            }
            else
            {
                //hide prices
                priceModel.OldPrice = null;
                priceModel.Price = null;
            }
        }

        private async Task PrepareGroupedProductOverviewPriceModelAsync(Product product,
            ProductOverviewModel.ProductPriceModel priceModel)
        {
            var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
                (await _storeContext.GetCurrentStoreAsync()).Id);

            //add to cart button (ignore "DisableBuyButton" property for grouped products)
            priceModel.DisableBuyButton =
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

            //add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
            priceModel.DisableWishlistButton =
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

            //compare products
            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
            if (!associatedProducts.Any())
                return;

            //we have at least one associated product
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                //find a minimum possible price
                decimal? minPossiblePrice = null;
                Product minPriceProduct = null;
                foreach (var associatedProduct in associatedProducts)
                {
                    var (_, tmpMinPossiblePrice, _, _) =
                        await _priceCalculationService.GetFinalPriceAsync(associatedProduct,
                            await _workContext.GetCurrentCustomerAsync());

                    if (associatedProduct.HasTierPrices)
                        //calculate price for the maximum quantity if we have tier prices, and choose minimal
                        tmpMinPossiblePrice = Math.Min(tmpMinPossiblePrice,
                            (await _priceCalculationService.GetFinalPriceAsync(associatedProduct,
                                await _workContext.GetCurrentCustomerAsync(), quantity: int.MaxValue)).Item1);

                    if (minPossiblePrice.HasValue && tmpMinPossiblePrice >= minPossiblePrice.Value)
                        continue;
                    minPriceProduct = associatedProduct;
                    minPossiblePrice = tmpMinPossiblePrice;
                }

                if (minPriceProduct == null || minPriceProduct.CustomerEntersPrice)
                    return;

                if (minPriceProduct.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                     _workContext.OriginalCustomerIfImpersonated == null))
                {
                    priceModel.OldPrice = null;
                    priceModel.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
                }
                else
                {
                    //calculate prices
                    var (finalPriceBase, _) =
                        await _taxService.GetProductPriceAsync(minPriceProduct, minPossiblePrice.Value);
                    var finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceBase,
                        await _workContext.GetWorkingCurrencyAsync());

                    priceModel.OldPrice = null;
                    priceModel.Price =
                        string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"),
                            await _priceFormatter.FormatPriceAsync(finalPrice));
                    priceModel.PriceValue = finalPrice;

                    //PAngV default baseprice (used in Germany)
                    priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceBase);
                }
            }
            else
            {
                //hide prices
                priceModel.OldPrice = null;
                priceModel.Price = null;
            }
        }

        protected virtual async Task<IList<ProductSpecificationAttributeModel>>
            PrepareProductSpecificationAttributeModelAsync(Product product, SpecificationAttributeGroup group)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productSpecificationAttributes =
                await _specificationAttributeService.GetProductSpecificationAttributesAsync(
                    product.Id, specificationAttributeGroupId: group?.Id, showOnProductPage: true);

            var result = new List<ProductSpecificationAttributeModel>();

            foreach (var psa in productSpecificationAttributes)
            {
                var option =
                    await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(
                        psa.SpecificationAttributeOptionId);

                var model = result.FirstOrDefault(model => model.Id == option.SpecificationAttributeId);
                if (model == null)
                {
                    var attribute =
                        await _specificationAttributeService.GetSpecificationAttributeByIdAsync(
                            option.SpecificationAttributeId);
                    model = new ProductSpecificationAttributeModel
                    {
                        Id = attribute.Id,
                        Name = await _localizationService.GetLocalizedAsync(attribute, x => x.Name)
                    };
                    result.Add(model);
                }

                var value = new ProductSpecificationAttributeValueModel
                {
                    AttributeTypeId = psa.AttributeTypeId,
                    ColorSquaresRgb = option.ColorSquaresRgb,
                    ValueRaw = psa.AttributeType switch
                    {
                        SpecificationAttributeType.Option => WebUtility.HtmlEncode(
                            await _localizationService.GetLocalizedAsync(option, x => x.Name)),
                        SpecificationAttributeType.CustomText => WebUtility.HtmlEncode(
                            await _localizationService.GetLocalizedAsync(psa, x => x.CustomValue)),
                        SpecificationAttributeType.CustomHtmlText => await _localizationService.GetLocalizedAsync(psa,
                            x => x.CustomValue),
                        SpecificationAttributeType.Hyperlink =>
                            $"<a href='{psa.CustomValue}' target='_blank'>{psa.CustomValue}</a>",
                        _ => null
                    }
                };

                model.Values.Add(value);
            }

            return result;
        }
    }
}