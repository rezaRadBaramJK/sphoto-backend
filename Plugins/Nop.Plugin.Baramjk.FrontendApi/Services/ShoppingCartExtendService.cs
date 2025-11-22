using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class ShoppingCartExtendService : ShoppingCartService
    {
        private readonly IAclService _aclService;
        private readonly ICurrencyService _currencyService;
        private readonly IDateRangeService _dateRangeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly OrderSettings _orderSettings;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;

        public ShoppingCartExtendService(CatalogSettings catalogSettings, IAclService aclService,
            IActionContextAccessor actionContextAccessor, ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService, ICurrencyService currencyService,
            ICustomerService customerService, IDateRangeService dateRangeService, IDateTimeHelper dateTimeHelper,
            IGenericAttributeService genericAttributeService, ILocalizationService localizationService,
            IPermissionService permissionService, IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter, IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService, IProductService productService,
            IRepository<ShoppingCartItem> sciRepository, IShippingService shippingService,
            IStaticCacheManager staticCacheManager, IStoreContext storeContext,
            IStoreMappingService storeMappingService, IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService, IWorkContext workContext, OrderSettings orderSettings,
            ShoppingCartSettings shoppingCartSettings, ILogger logger) : base(catalogSettings, aclService,
            actionContextAccessor,
            checkoutAttributeParser, checkoutAttributeService, currencyService, customerService, dateRangeService,
            dateTimeHelper, genericAttributeService, localizationService, permissionService, priceCalculationService,
            priceFormatter, productAttributeParser, productAttributeService, productService, sciRepository,
            shippingService, staticCacheManager, storeContext, storeMappingService, urlHelperFactory, urlRecordService,
            workContext, orderSettings, shoppingCartSettings)
        {
            _aclService = aclService;
            _currencyService = currencyService;
            _dateRangeService = dateRangeService;
            _localizationService = localizationService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _logger = logger;
            //logger.ErrorAsync("aaa");
        }

        protected override async Task<IList<string>> GetStandardWarningsAsync(Customer customer,
            ShoppingCartType shoppingCartType, Product product, string attributesXml, decimal customerEnteredPrice,
            int quantity, int shoppingCartItemId, int storeId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();

            //deleted
            if (product.Deleted)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.ProductDeleted"));
                return warnings;
            }

            //published
            /*if (!product.Published)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.ProductUnpublished"));
            }*/

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct) warnings.Add("This is not simple product");

            //ACL
            if (!await _aclService.AuthorizeAsync(product, customer))
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.ProductUnpublished"));

            //Store mapping
            if (!await _storeMappingService.AuthorizeAsync(product, (await _storeContext.GetCurrentStoreAsync()).Id))
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.ProductUnpublished"));

            //disabled "add to cart" button
            if (shoppingCartType == ShoppingCartType.ShoppingCart && product.DisableBuyButton)
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.BuyingDisabled"));

            //disabled "add to wishlist" button
            if (shoppingCartType == ShoppingCartType.Wishlist && product.DisableWishlistButton)
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.WishlistDisabled"));

            //call for price
            if (shoppingCartType == ShoppingCartType.ShoppingCart && product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                 _workContext.OriginalCustomerIfImpersonated == null))
                warnings.Add(await _localizationService.GetResourceAsync("Products.CallForPrice"));

            //customer entered price
            if (product.CustomerEntersPrice)
                if (customerEnteredPrice < product.MinimumCustomerEnteredPrice ||
                    customerEnteredPrice > product.MaximumCustomerEnteredPrice)
                {
                    var minimumCustomerEnteredPrice =
                        await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MinimumCustomerEnteredPrice,
                            await _workContext.GetWorkingCurrencyAsync());
                    var maximumCustomerEnteredPrice =
                        await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MaximumCustomerEnteredPrice,
                            await _workContext.GetWorkingCurrencyAsync());
                    warnings.Add(string.Format(
                        await _localizationService.GetResourceAsync("ShoppingCart.CustomerEnteredPrice.RangeError"),
                        await _priceFormatter.FormatPriceAsync(minimumCustomerEnteredPrice, false, false),
                        await _priceFormatter.FormatPriceAsync(maximumCustomerEnteredPrice, false, false)));
                }

            //quantity validation
            var hasQtyWarnings = false;
            if (quantity < product.OrderMinimumQuantity)
            {
                warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MinimumQuantity"),
                    product.OrderMinimumQuantity));
                hasQtyWarnings = true;
            }

            if (quantity > product.OrderMaximumQuantity)
            {
                warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumQuantity"),
                    product.OrderMaximumQuantity));
                hasQtyWarnings = true;
            }

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            if (allowedQuantities.Length > 0 && !allowedQuantities.Contains(quantity))
                warnings.Add(string.Format(
                    await _localizationService.GetResourceAsync("ShoppingCart.AllowedQuantities"),
                    string.Join(", ", allowedQuantities)));

            var validateOutOfStock = shoppingCartType == ShoppingCartType.ShoppingCart ||
                                     !_shoppingCartSettings.AllowOutOfStockItemsToBeAddedToWishlist;
            if (validateOutOfStock && !hasQtyWarnings)
                switch (product.ManageInventoryMethod)
                {
                    case ManageInventoryMethod.DontManageStock:
                        //do nothing
                        break;
                    case ManageInventoryMethod.ManageStock:
                        if (product.BackorderMode == BackorderMode.NoBackorders)
                        {
                            var maximumQuantityCanBeAdded = await _productService.GetTotalStockQuantityAsync(product);

                            warnings.AddRange(await GetQuantityProductWarningsAsync(product, quantity,
                                maximumQuantityCanBeAdded));

                            if (warnings.Any())
                                return warnings;

                            //validate product quantity with non combinable product attributes
                            var productAttributeMappings =
                                await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                            if (productAttributeMappings?.Any() == true)
                            {
                                var onlyCombinableAttributes =
                                    productAttributeMappings.All(mapping => !mapping.IsNonCombinable());
                                if (!onlyCombinableAttributes)
                                {
                                    var cart = await GetShoppingCartAsync(customer, shoppingCartType, storeId);
                                    var totalAddedQuantity = cart
                                        .Where(item => item.ProductId == product.Id && item.Id != shoppingCartItemId)
                                        .Sum(product => product.Quantity);

                                    totalAddedQuantity += quantity;

                                    //counting a product into bundles
                                    foreach (var bundle in cart.Where(x =>
                                                 x.Id != shoppingCartItemId && !string.IsNullOrEmpty(x.AttributesXml)))
                                    {
                                        var attributeValues =
                                            await _productAttributeParser.ParseProductAttributeValuesAsync(
                                                bundle.AttributesXml);
                                        foreach (var attributeValue in attributeValues)
                                            if (attributeValue.AttributeValueType ==
                                                AttributeValueType.AssociatedToProduct &&
                                                attributeValue.AssociatedProductId == product.Id)
                                                totalAddedQuantity += bundle.Quantity * attributeValue.Quantity;
                                    }

                                    warnings.AddRange(await GetQuantityProductWarningsAsync(product, totalAddedQuantity,
                                        maximumQuantityCanBeAdded));
                                }
                            }

                            if (warnings.Any())
                                return warnings;

                            //validate product quantity and product quantity into bundles
                            if (string.IsNullOrEmpty(attributesXml))
                            {
                                var cart = await GetShoppingCartAsync(customer, shoppingCartType, storeId);
                                var totalQuantityInCart = cart.Where(item =>
                                        item.ProductId == product.Id && item.Id != shoppingCartItemId &&
                                        string.IsNullOrEmpty(item.AttributesXml))
                                    .Sum(product => product.Quantity);

                                totalQuantityInCart += quantity;

                                foreach (var bundle in cart.Where(x =>
                                             x.Id != shoppingCartItemId && !string.IsNullOrEmpty(x.AttributesXml)))
                                {
                                    var attributeValues =
                                        await _productAttributeParser.ParseProductAttributeValuesAsync(
                                            bundle.AttributesXml);
                                    foreach (var attributeValue in attributeValues)
                                        if (attributeValue.AttributeValueType ==
                                            AttributeValueType.AssociatedToProduct &&
                                            attributeValue.AssociatedProductId == product.Id)
                                            totalQuantityInCart += bundle.Quantity * attributeValue.Quantity;
                                }

                                warnings.AddRange(await GetQuantityProductWarningsAsync(product, totalQuantityInCart,
                                    maximumQuantityCanBeAdded));
                            }
                        }

                        break;
                    case ManageInventoryMethod.ManageStockByAttributes:
                        var combination =
                            await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
                        if (combination != null)
                        {
                            //combination exists
                            //let's check stock level
                            if (!combination.AllowOutOfStockOrders)
                                warnings.AddRange(await GetQuantityProductWarningsAsync(product, quantity,
                                    combination.StockQuantity));
                        }
                        else
                        {
                            //combination doesn't exist
                            if (product.AllowAddingOnlyExistingAttributeCombinations)
                            {
                                //maybe, is it better  to display something like "No such product/combination" message?
                                var productAvailabilityRange =
                                    await _dateRangeService.GetProductAvailabilityRangeByIdAsync(
                                        product.ProductAvailabilityRangeId);
                                var warning = productAvailabilityRange == null
                                    ? await _localizationService.GetResourceAsync("ShoppingCart.OutOfStock")
                                    : string.Format(
                                        await _localizationService.GetResourceAsync("ShoppingCart.AvailabilityRange"),
                                        await _localizationService.GetLocalizedAsync(productAvailabilityRange,
                                            range => range.Name));
                                warnings.Add(warning);
                            }
                        }

                        break;
                }

            //availability dates
            var availableStartDateError = false;
            if (product.AvailableStartDateTimeUtc.HasValue)
            {
                var availableStartDateTime =
                    DateTime.SpecifyKind(product.AvailableStartDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableStartDateTime.CompareTo(DateTime.UtcNow) > 0)
                {
                    warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.NotAvailable"));
                    availableStartDateError = true;
                }
            }

            if (!product.AvailableEndDateTimeUtc.HasValue || availableStartDateError)
                return warnings;

            var availableEndDateTime = DateTime.SpecifyKind(product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc);
            if (availableEndDateTime.CompareTo(DateTime.UtcNow) < 0)
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.NotAvailable"));

            return warnings;
        }
    }
}