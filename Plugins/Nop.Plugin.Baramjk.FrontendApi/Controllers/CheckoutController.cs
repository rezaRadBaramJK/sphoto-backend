using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Addresses.Abstractions;
using Nop.Plugin.Baramjk.FrontendApi.Dto;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Baramjk.FrontendApi.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class SummeryModel
    {
        public CheckoutPaymentMethodModel.PaymentMethodModel PaymentMethod { get; set; }
        public OrderTotalsModel TotalsModel { get; set; }
    }

    public class CheckoutController : BaseNopWebApiFrontendController
    {
        #region Ctor

        public CheckoutController(AddressSettings addressSettings,
            CustomerSettings customerSettings,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressService addressService,
            ICheckoutModelFactory checkoutModelFactory,
            ICountryService countryService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IProductService productService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings, IShoppingCartModelFactory shoppingCartModelFactory,
            IPriceFormatter priceFormatter, FrontendApiSettings frontendApiSettings, IFakeAddressService fakeAddressService)
        {
            _addressSettings = addressSettings;
            _customerSettings = customerSettings;
            _addressAttributeParser = addressAttributeParser;
            _addressAttributeService = addressAttributeService;
            _addressService = addressService;
            _checkoutModelFactory = checkoutModelFactory;
            _countryService = countryService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _productService = productService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _paymentSettings = paymentSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _priceFormatter = priceFormatter;
            _frontendApiSettings = frontendApiSettings;
            _fakeAddressService = fakeAddressService;
        }

        #endregion

        #region Fields

        private readonly IPriceFormatter _priceFormatter;

        private readonly AddressSettings _addressSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressService _addressService;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IProductService _productService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly FrontendApiSettings _frontendApiSettings;
        private readonly IFakeAddressService _fakeAddressService;

        #endregion

        #region Utilities

        protected virtual async Task<IActionResult> OpcLoadStepAfterShippingAddress(IList<ShoppingCartItem> cart)
        {
            var shippingMethodModel = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart,
                await _customerService.GetCustomerShippingAddressAsync(await _workContext.GetCurrentCustomerAsync()));
            if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
                shippingMethodModel.ShippingMethods.Count == 1)
            {
                //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedShippingOptionAttribute,
                    shippingMethodModel.ShippingMethods.First().ShippingOption,
                    (await _storeContext.GetCurrentStoreAsync()).Id);

                //load next step
                return await OpcLoadStepAfterShippingMethod(cart);
            }

            return ApiResponseFactory.Success(new NextStepResponse<CheckoutShippingMethodModelDto>
            {
                UpdateSectionModel = new UpdateSectionJsonModelDto<CheckoutShippingMethodModelDto>
                {
                    Name = "shipping-method",
                    ViewName = "OpcShippingMethods",
                    Model = shippingMethodModel.ToDto<CheckoutShippingMethodModelDto>()
                },
                GotoSection = "shipping_method"
            });
        }

        protected virtual async Task<IActionResult> OpcLoadStepAfterShippingMethod(IList<ShoppingCartItem> cart)
        {
            //Check whether payment workflow is required
            //we ignore reward points during cart total calculation
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
            if (isPaymentWorkflowRequired)
            {
                //filter by country
                var filterByCountryId = 0;
                if (_addressSettings.CountryEnabled)
                    filterByCountryId =
                        (await _customerService.GetCustomerBillingAddressAsync(
                            await _workContext.GetCurrentCustomerAsync()))?.CountryId ?? 0;

                //payment is required
                var paymentMethodModel =
                    await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);

                if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                    paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
                {
                    //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
                    //so customer doesn't have to choose a payment method

                    var selectedPaymentMethodSystemName = paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName;
                    await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                        NopCustomerDefaults.SelectedPaymentMethodAttribute,
                        selectedPaymentMethodSystemName, (await _storeContext.GetCurrentStoreAsync()).Id);

                    var paymentMethodInst = await _paymentPluginManager
                        .LoadPluginBySystemNameAsync(selectedPaymentMethodSystemName,
                            await _workContext.GetCurrentCustomerAsync(),
                            (await _storeContext.GetCurrentStoreAsync()).Id);
                    if (!_paymentPluginManager.IsPluginActive(paymentMethodInst))
                        return ApiResponseFactory.BadRequest("Selected payment method can't be parsed");

                    return await OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
                }

                //customer have to choose a payment method
                return ApiResponseFactory.Success(new NextStepResponse<CheckoutPaymentMethodModelDto>
                {
                    UpdateSectionModel = new UpdateSectionJsonModelDto<CheckoutPaymentMethodModelDto>
                    {
                        Name = "payment-method",
                        ViewName = "OpcPaymentMethods",
                        Model = paymentMethodModel.ToDto<CheckoutPaymentMethodModelDto>()
                    },
                    GotoSection = "payment_method"
                });
            }

            //payment is not required
            await _genericAttributeService.SaveAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.SelectedPaymentMethodAttribute, null,
                (await _storeContext.GetCurrentStoreAsync()).Id);

            var confirmOrderModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
            return ApiResponseFactory.Success(new NextStepResponse<CheckoutConfirmModelDto>
            {
                UpdateSectionModel = new UpdateSectionJsonModelDto<CheckoutConfirmModelDto>
                {
                    Name = "confirm-order",
                    ViewName = "OpcConfirmOrder",
                    Model = confirmOrderModel.ToDto<CheckoutConfirmModelDto>()
                },
                GotoSection = "confirm_order"
            });
        }

        protected virtual async Task<IActionResult> OpcLoadStepAfterPaymentMethod(IPaymentMethod paymentMethod,
            IList<ShoppingCartItem> cart)
        {
            if (paymentMethod.SkipPaymentInfo ||
                (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection &&
                 _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
            {
                //skip payment info page
                var paymentInfo = new ProcessPaymentRequest();

                //paymentInfo save
                await SavePaymentInfoAsync(paymentInfo);

                var confirmOrderModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
                return ApiResponseFactory.Success(new NextStepResponse<CheckoutConfirmModelDto>
                {
                    UpdateSectionModel = new UpdateSectionJsonModelDto<CheckoutConfirmModelDto>
                    {
                        Name = "confirm-order",
                        ViewName = "OpcConfirmOrder",
                        Model = confirmOrderModel.ToDto<CheckoutConfirmModelDto>()
                    },
                    GotoSection = "confirm_order"
                });
            }

            //return payment info page
            var paymenInfoModel = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);
            return ApiResponseFactory.Success(new NextStepResponse<CheckoutPaymentInfoModelDto>
            {
                UpdateSectionModel = new UpdateSectionJsonModelDto<CheckoutPaymentInfoModelDto>
                {
                    Name = "payment-info",
                    ViewName = "OpcPaymentInfo",
                    Model = paymenInfoModel.ToDto<CheckoutPaymentInfoModelDto>()
                },
                GotoSection = "payment_info"
            });
        }

        /// <summary>
        ///     Prepare payment info cash key
        /// </summary>
        /// <returns>Cache key</returns>
        protected virtual async Task<CacheKey> PreparePaymentInfoCachKey()
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(WebApiFrontendDefaults.PaymentInfoKeyCache,
                await _storeContext.GetCurrentStoreAsync(),
                (await _workContext.GetCurrentCustomerAsync()).CustomerGuid);
            return key;
        }

        /// <summary>
        ///     Save process payment request
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        protected virtual async Task SavePaymentInfoAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var key = await PreparePaymentInfoCachKey();
            await _staticCacheManager.SetAsync(key, processPaymentRequest);
        }

        /// <summary>
        ///     Get process payment request
        /// </summary>
        /// <returns>Process payment request</returns>
        protected virtual async Task<ProcessPaymentRequest> GetPaymentInfoAsync()
        {
            var key = await PreparePaymentInfoCachKey();
            return await _staticCacheManager.GetAsync(key, () => new ProcessPaymentRequest());
        }

        /// <summary>
        ///     Clear payment info
        /// </summary>
        protected virtual async Task ClearPaymentInfoAsync()
        {
            var key = await PreparePaymentInfoCachKey();
            await _staticCacheManager.RemoveAsync(key);
        }

        /// <summary>
        ///     Generate an order GUID
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        protected virtual async Task GenerateOrderGuidAsync(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                return;

            //we should use the same GUID for multiple payment attempts
            //this way a payment gateway can prevent security issues such as credit card brute-force attacks
            //in order to avoid any possible limitations by payment gateway we reset GUID periodically
            var previousPaymentRequest = await GetPaymentInfoAsync();
            if (_paymentSettings.RegenerateOrderGuidInterval > 0 &&
                previousPaymentRequest.OrderGuidGeneratedOnUtc.HasValue)
            {
                var interval = DateTime.UtcNow - previousPaymentRequest.OrderGuidGeneratedOnUtc.Value;
                if (interval.TotalSeconds < _paymentSettings.RegenerateOrderGuidInterval)
                {
                    processPaymentRequest.OrderGuid = previousPaymentRequest.OrderGuid;
                    processPaymentRequest.OrderGuidGeneratedOnUtc = previousPaymentRequest.OrderGuidGeneratedOnUtc;
                }
            }

            if (processPaymentRequest.OrderGuid == Guid.Empty)
            {
                processPaymentRequest.OrderGuid = Guid.NewGuid();
                processPaymentRequest.OrderGuidGeneratedOnUtc = DateTime.UtcNow;
            }
        }

        protected virtual async Task<bool> IsMinimumOrderPlacementIntervalValidAsync(Customer customer)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

            var lastOrder = (await _orderService.SearchOrdersAsync(
                    (await _storeContext.GetCurrentStoreAsync()).Id,
                    customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1))
                .FirstOrDefault();
            if (lastOrder == null)
                return true;

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }

        /// <summary>
        ///     Parses the value indicating whether the "pickup in store" is allowed
        /// </summary>
        /// <param name="form">The form</param>
        /// <returns>The value indicating whether the "pickup in store" is allowed</returns>
        protected virtual bool ParsePickupInStore(IDictionary<string, string> form)
        {
            var pickupInStore = false;

            var pickupInStoreParameter = form["PickupInStore"];
            if (!string.IsNullOrWhiteSpace(pickupInStoreParameter))
                _ = bool.TryParse(pickupInStoreParameter, out pickupInStore);

            return pickupInStore;
        }

        /// <summary>
        ///     Parses the pickup option
        /// </summary>
        /// <param name="form">The form</param>
        /// <returns>
        ///     A task that represents the asynchronous operation
        ///     The task result contains the pickup option
        /// </returns>
        protected virtual async Task<PickupPoint> ParsePickupOptionAsync(IDictionary<string, string> form)
        {
            var pickupPoint = form["pickup-points-id"].Split(new[] { "___" }, StringSplitOptions.None);

            var selectedPoint = (await _shippingService.GetPickupPointsAsync(
                    (await _workContext.GetCurrentCustomerAsync()).BillingAddressId ?? 0,
                    await _workContext.GetCurrentCustomerAsync(), pickupPoint[1],
                    (await _storeContext.GetCurrentStoreAsync()).Id)).PickupPoints
                .FirstOrDefault(x => x.Id.Equals(pickupPoint[0]));

            if (selectedPoint == null)
                throw new Exception("Pickup point is not allowed");

            return selectedPoint;
        }

        /// <summary>
        ///     Saves the pickup option
        /// </summary>
        /// <param name="pickupPoint">The pickup option</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task SavePickupOptionAsync(PickupPoint pickupPoint)
        {
            var name = !string.IsNullOrEmpty(pickupPoint.Name)
                ? string.Format(await _localizationService.GetResourceAsync("Checkout.PickupPoints.Name"),
                    pickupPoint.Name)
                : await _localizationService.GetResourceAsync("Checkout.PickupPoints.NullName");
            var pickUpInStoreShippingOption = new ShippingOption
            {
                Name = name,
                Rate = pickupPoint.PickupFee,
                Description = pickupPoint.Description,
                ShippingRateComputationMethodSystemName = pickupPoint.ProviderSystemName,
                IsPickupInStore = true
            };

            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.SelectedShippingOptionAttribute, pickUpInStoreShippingOption,
                (await _storeContext.GetCurrentStoreAsync()).Id);
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.SelectedPickupPointAttribute, pickupPoint,
                (await _storeContext.GetCurrentStoreAsync()).Id);
        }

        /// <summary>
        ///     Get custom address attributes from the passed form
        /// </summary>
        /// <param name="form">Form values</param>
        /// <returns>
        ///     A task that represents the asynchronous operation
        ///     The task result contains the attributes in XML format
        /// </returns>
        protected virtual async Task<string> ParseCustomAddressAttributesAsync(IDictionary<string, string> form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;

            foreach (var attribute in await _addressAttributeService.GetAllAddressAttributesAsync())
            {
                var controlId = string.Format(NopCommonDefaults.AddressAttributeControlName, attribute.Id);
                var attributeValues = form[controlId];
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        if (!StringValues.IsNullOrEmpty(attributeValues) &&
                            int.TryParse(attributeValues, out var value) && value > 0)
                            attributesXml =
                                _addressAttributeParser.AddAddressAttribute(attributesXml, attribute, value.ToString());
                        break;

                    case AttributeControlType.Checkboxes:
                        foreach (var attributeValue in attributeValues.Split(new[] { ',' },
                                     StringSplitOptions.RemoveEmptyEntries))
                            if (int.TryParse(attributeValue, out value) && value > 0)
                                attributesXml =
                                    _addressAttributeParser.AddAddressAttribute(attributesXml, attribute,
                                        value.ToString());

                        break;

                    case AttributeControlType.ReadonlyCheckboxes:
                        //load read-only (already server-side selected) values
                        var addressAttributeValues =
                            await _addressAttributeService.GetAddressAttributeValuesAsync(attribute.Id);
                        foreach (var addressAttributeValue in addressAttributeValues)
                            if (addressAttributeValue.IsPreSelected)
                                attributesXml = _addressAttributeParser.AddAddressAttribute(attributesXml, attribute,
                                    addressAttributeValue.Id.ToString());

                        break;

                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        if (!StringValues.IsNullOrEmpty(attributeValues))
                            attributesXml =
                                _addressAttributeParser.AddAddressAttribute(attributesXml, attribute,
                                    attributeValues.Trim());
                        break;

                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        #endregion

        #region Methods (common)

        /// <summary>
        ///     Index
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CheckoutBillingAddressModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Index()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.CheckoutDisabled)} is  enabled.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest(new List<string> { "Your cart is empty" });

            var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();
            var downloadableProductsRequireRegistration =
                _customerSettings.RequireRegistrationForDownloadableProducts &&
                await _productService.HasAnyDownloadableProductAsync(cartProductIds);

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                (!_orderSettings.AnonymousCheckoutAllowed || downloadableProductsRequireRegistration))
                return ApiResponseFactory.BadRequest(new List<string> { "Customer is not registered." });

            //if we have only "button" payment methods available (displayed on the shopping cart page, not during checkout),
            //then we should allow standard checkout
            //all payment methods (do not filter by country here as it could be not specified yet)
            var paymentMethods = await (await _paymentPluginManager
                    .LoadActivePluginsAsyncAsync(await _workContext.GetCurrentCustomerAsync(),
                        (await _storeContext.GetCurrentStoreAsync()).Id))
                .WhereAwait(async pm => !await pm.HidePaymentMethodAsync(cart)).ToListAsync();
            //payment methods displayed during checkout (not with "Button" type)
            var nonButtonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType != PaymentMethodType.Button)
                .ToList();
            //"button" payment methods(*displayed on the shopping cart page)
            var buttonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .ToList();
            if (!nonButtonPaymentMethods.Any() && buttonPaymentMethods.Any())
                return ApiResponseFactory.NotFound("Not found payment methods displayed during checkout.");

            //reset checkout data
            await _customerService.ResetCheckoutDataAsync(await _workContext.GetCurrentCustomerAsync(),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            //validation (cart)
            var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(
                await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.CheckoutAttributes, (await _storeContext.GetCurrentStoreAsync()).Id);
            var scWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, checkoutAttributesXml, true);
            if (scWarnings.Any())
                return ApiResponseFactory.BadRequest(scWarnings);

            //validation (each shopping cart item)
            foreach (var sci in cart)
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                var sciWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(
                    await _workContext.GetCurrentCustomerAsync(),
                    sci.ShoppingCartType,
                    product,
                    sci.StoreId,
                    sci.AttributesXml,
                    sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc,
                    sci.RentalEndDateUtc,
                    sci.Quantity,
                    false,
                    sci.Id);
                if (sciWarnings.Any())
                    return ApiResponseFactory.BadRequest(sciWarnings);
            }

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            //model
            var model = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart,
                prePopulateNewAddressWithCustomerFields: true);

            //check whether "billing address" step is enabled
            if (_orderSettings.DisableBillingAddressCheckoutStep && model.ExistingAddresses.Any())
            {
                if (model.ExistingAddresses.Any())
                    //choose the first one
                    return await SelectBillingAddress(model.ExistingAddresses.First().Id);

                TryValidateModel(model);
                TryValidateModel(model.BillingNewAddress);
                return await NewBillingAddress(new BaseModelDtoRequest<CheckoutBillingAddressModelDto>
                    { Model = model.ToDto<CheckoutBillingAddressModelDto>() });
            }

            return ApiResponseFactory.Success(model.ToDto<CheckoutBillingAddressModelDto>());
        }

        /// <summary>
        ///     Prepare checkout completed model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CompletedResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Completed([FromQuery] int? orderId)
        {
            //validation
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            Order order = null;
            if (orderId.HasValue)
                //load order by identifier (if provided)
                order = await _orderService.GetOrderByIdAsync(orderId.Value);
            if (order == null)
                order = (await _orderService.SearchOrdersAsync((await _storeContext.GetCurrentStoreAsync()).Id,
                        customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1))
                    .FirstOrDefault();

            if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return ApiResponseFactory.NotFound("Order not found or does not meet the requirements.");

            //disable "order completed" page?
            if (_orderSettings.DisableOrderCompletedPage)
                return ApiResponseFactory.Success(new CompletedResponse
                    { RedirectToMethod = "Order_Details", Id = order.Id });

            //model
            var model = await _checkoutModelFactory.PrepareCheckoutCompletedModelAsync(order);

            return ApiResponseFactory.Success(
                new CompletedResponse { Model = model.ToDto<CheckoutCompletedModelDto>() });
        }


        /// <summary>
        ///     Get specified Address by addresId
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        [HttpGet("{addressId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GetAddressByIdResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAddressById(int addressId)
        {
            var address =
                await _customerService.GetCustomerAddressAsync((await _workContext.GetCurrentCustomerAsync()).Id,
                    addressId);
            if (address == null)
                return ApiResponseFactory.NotFound($"Address by id={addressId} not found.");

            var json = JsonConvert.SerializeObject(address, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            return ApiResponseFactory.Success(new GetAddressByIdResponse
            {
                Content = json,
                ContentType = "application/json"
            });
        }

        /// <summary>
        ///     Save edited address
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EditAddressResponse<CheckoutBillingAddressModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SaveEditAddress([FromBody] CheckoutBillingAddressModelDto requestModel,
            [FromQuery] [Required] bool opc)
        {
            try
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                    ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (!cart.Any())
                    return ApiResponseFactory.BadRequest("Your cart is empty");

                var model = requestModel.FromDto<CheckoutBillingAddressModel>();

                var customer = await _workContext.GetCurrentCustomerAsync();
                //find address (ensure that it belongs to the current customer)
                var address = await _customerService.GetCustomerAddressAsync(customer.Id, model.BillingNewAddress.Id);
                if (address == null)
                    return ApiResponseFactory.NotFound($"Address by id={model.BillingNewAddress.Id} not found.");

                address = model.BillingNewAddress.ToEntity(address);
                await _addressService.UpdateAddressAsync(address);

                (await _workContext.GetCurrentCustomerAsync()).BillingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

                if (!opc)
                    return ApiResponseFactory.Success(new EditAddressResponse<CheckoutBillingAddressModelDto>
                    {
                        Redirect = Url.RouteUrl("CheckoutBillingAddress")
                    });

                var billingAddressModel =
                    await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart, address.CountryId);
                return ApiResponseFactory.Success(new EditAddressResponse<CheckoutBillingAddressModelDto>
                {
                    SelectedId = model.BillingNewAddress.Id,
                    UpdateSectionModel = new UpdateSectionJsonModelDto<CheckoutBillingAddressModelDto>
                    {
                        Name = "billing",
                        ViewName = "OpcBillingAddress",
                        Model = billingAddressModel.ToDto<CheckoutBillingAddressModelDto>()
                    }
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return ApiResponseFactory.BadRequest(exc.Message);
            }
        }

        /// <summary>
        ///     Delete edited address
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        /// <param name="opc">Is one page checkout</param>
        [HttpDelete("{addressId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EditAddressResponse<CheckoutBillingAddressModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> DeleteEditAddress(int addressId, [FromQuery] bool opc = false)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            var customer = await _workContext.GetCurrentCustomerAsync();

            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address != null)
            {
                await _customerService.RemoveCustomerAddressAsync(customer, address);
                await _customerService.UpdateCustomerAsync(customer);
                await _addressService.DeleteAddressAsync(address);
            }

            if (!opc)
                return ApiResponseFactory.Success(new EditAddressResponse<CheckoutBillingAddressModelDto>
                {
                    Redirect = Url.RouteUrl("CheckoutBillingAddress")
                });

            var billingAddressModel = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart);
            return ApiResponseFactory.Success(new EditAddressResponse<CheckoutBillingAddressModelDto>
            {
                UpdateSectionModel = new UpdateSectionJsonModelDto<CheckoutBillingAddressModelDto>
                {
                    Name = "billing",
                    ViewName = "OpcBillingAddress",
                    Model = billingAddressModel.ToDto<CheckoutBillingAddressModelDto>()
                }
            });
        }

        #endregion

        #region Methods (multistep checkout)

        /// <summary>
        ///     Prepare billing address model
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CheckoutBillingAddressModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> BillingAddress([FromBody] IDictionary<string, string> form)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.CheckoutDisabled)} is  enabled.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            //model
            var model = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart,
                prePopulateNewAddressWithCustomerFields: true);

            //check whether "billing address" step is enabled
            if (_orderSettings.DisableBillingAddressCheckoutStep && model.ExistingAddresses.Any())
            {
                if (model.ExistingAddresses.Any())
                    //choose the first one
                    return await SelectBillingAddress(model.ExistingAddresses.First().Id);

                TryValidateModel(model);
                TryValidateModel(model.BillingNewAddress);
                return await NewBillingAddress(new BaseModelDtoRequest<CheckoutBillingAddressModelDto>
                    { Model = model.ToDto<CheckoutBillingAddressModelDto>(), Form = form });
            }

            return ApiResponseFactory.Success(model.ToDto<CheckoutBillingAddressModelDto>());
        }

        /// <summary>
        ///     Select billing address
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        /// <param name="shipToSameAddress">A value indicating "Ship to the same address" option is enabled</param>
        [HttpGet("{addressId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CheckoutRedirectResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SelectBillingAddress(int addressId,
            [FromQuery] bool shipToSameAddress = false)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var address =
                await _customerService.GetCustomerAddressAsync((await _workContext.GetCurrentCustomerAsync()).Id,
                    addressId);
            if (address == null)
                return ApiResponseFactory.NotFound($"Address by id={addressId} not found.");

            (await _workContext.GetCurrentCustomerAsync()).BillingAddressId = address.Id;
            await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            //ship to the same address?
            //by default Shipping is available if the country is not specified
            var shippingAllowed = !_addressSettings.CountryEnabled ||
                                  ((await _countryService.GetCountryByAddressAsync(address))?.AllowsShipping ?? false);
            if (_shippingSettings.ShipToSameAddress && shipToSameAddress &&
                await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart) && shippingAllowed)
            {
                (await _workContext.GetCurrentCustomerAsync()).ShippingAddressId =
                    (await _workContext.GetCurrentCustomerAsync()).BillingAddressId;
                await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());
                //reset selected shipping method (in case if "pick up in store" was selected)
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(
                    await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute,
                    null, (await _storeContext.GetCurrentStoreAsync()).Id);
                await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                    await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute,
                    null, (await _storeContext.GetCurrentStoreAsync()).Id);
                //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 
                return ApiResponseFactory.Success(new CheckoutRedirectResponse { RedirectToMethod = "ShippingMethod" });
            }

            return ApiResponseFactory.Success(new CheckoutRedirectResponse { RedirectToMethod = "ShippingAddress" });
        }

        /// <summary>
        ///     New billing address
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NewBillingAddressResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> NewBillingAddress(
            [FromBody] BaseModelDtoRequest<CheckoutBillingAddressModelDto> request)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            //custom address attributes
            var customAttributes = await ParseCustomAddressAttributesAsync(request.Form);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);

            var errors = new List<string>();
            errors.AddRange(customAttributeWarnings);

            var model = request.Model.FromDto<CheckoutBillingAddressModel>();

            var newAddress = model.BillingNewAddress;

            if (!errors.Any())
            {
                //try to find an address with the same values (don't duplicate records)
                var address = _addressService.FindAddress(
                    (await _customerService.GetAddressesByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync())
                        .Id)).ToList(),
                    newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                    newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                    newAddress.Address1, newAddress.Address2, newAddress.City,
                    newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                    newAddress.CountryId, customAttributes);

                if (address == null)
                {
                    //address is not found. let's create a new one
                    address = newAddress.ToEntity();
                    address.CustomAttributes = customAttributes;
                    address.CreatedOnUtc = DateTime.UtcNow;

                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    await _addressService.InsertAddressAsync(address);

                    await _customerService.InsertCustomerAddressAsync(await _workContext.GetCurrentCustomerAsync(),
                        address);
                }

                (await _workContext.GetCurrentCustomerAsync()).BillingAddressId = address.Id;

                await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

                //ship to the same address?
                if (_shippingSettings.ShipToSameAddress && model.ShipToSameAddress &&
                    await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                {
                    (await _workContext.GetCurrentCustomerAsync()).ShippingAddressId =
                        (await _workContext.GetCurrentCustomerAsync()).BillingAddressId;
                    await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

                    //reset selected shipping method (in case if "pick up in store" was selected)
                    await _genericAttributeService.SaveAttributeAsync<ShippingOption>(
                        await _workContext.GetCurrentCustomerAsync(),
                        NopCustomerDefaults.SelectedShippingOptionAttribute, null,
                        (await _storeContext.GetCurrentStoreAsync()).Id);
                    await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                        await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute,
                        null, (await _storeContext.GetCurrentStoreAsync()).Id);

                    //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 
                    return ApiResponseFactory.Success(new NewBillingAddressResponse
                        { RedirectToMethod = "ShippingMethod" });
                }

                return ApiResponseFactory.Success(
                    new NewBillingAddressResponse { RedirectToMethod = "ShippingAddress" });
            }

            //If we got this far, something failed, redisplay form
            model = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart,
                newAddress.CountryId,
                overrideAttributesXml: customAttributes);

            return ApiResponseFactory.Success(new NewBillingAddressResponse
                { Model = model.ToDto<CheckoutBillingAddressModelDto>() });
        }

        /// <summary>
        ///     Prepare shipping address model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ShippingAddressResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ShippingAddress()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                return ApiResponseFactory.Success(new ShippingAddressResponse { RedirectToMethod = "ShippingMethod" });

            //model
            var model = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart,
                prePopulateNewAddressWithCustomerFields: true);
            return ApiResponseFactory.Success(new ShippingAddressResponse
                { Model = model.ToDto<CheckoutShippingAddressModelDto>() });
        }

        /// <summary>
        ///     Select shipping address
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        [HttpGet("{addressId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CheckoutRedirectResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SelectShippingAddress(int addressId)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var address =
                await _customerService.GetCustomerAddressAsync((await _workContext.GetCurrentCustomerAsync()).Id,
                    addressId);
            if (address == null)
                return ApiResponseFactory.NotFound($"Address by id={addressId} not found.");

            (await _workContext.GetCurrentCustomerAsync()).ShippingAddressId = address.Id;
            await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

            if (_shippingSettings.AllowPickupInStore)
                //set value indicating that "pick up in store" option has not been chosen
                await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                    await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute,
                    null, (await _storeContext.GetCurrentStoreAsync()).Id);

            return ApiResponseFactory.Success(new CheckoutRedirectResponse { RedirectToMethod = "ShippingMethod" });
        }

        /// <summary>
        ///     New shipping address
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NewShippingAddressResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> NewShippingAddress(
            [FromBody] BaseModelDtoRequest<CheckoutShippingAddressModelDto> request)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                return ApiResponseFactory.Success(
                    new NewShippingAddressResponse { RedirectToMethod = "ShippingMethod" });

            //pickup point
            if (_shippingSettings.AllowPickupInStore && !_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
            {
                var pickupInStore = ParsePickupInStore(request.Form);
                if (pickupInStore)
                {
                    var pickupOption = await ParsePickupOptionAsync(request.Form);
                    await SavePickupOptionAsync(pickupOption);

                    return ApiResponseFactory.Success(new NewShippingAddressResponse
                        { RedirectToMethod = "PaymentMethod" });
                }

                //set value indicating that "pick up in store" option has not been chosen
                await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                    await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute,
                    null, (await _storeContext.GetCurrentStoreAsync()).Id);
            }

            //custom address attributes
            var customAttributes = await ParseCustomAddressAttributesAsync(request.Form);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);

            var errors = new List<string>();
            errors.AddRange(customAttributeWarnings);

            //var model = request.Model.FromDto<CheckoutShippingAddressModelDto>();
            var newAddress = request.Model.ShippingNewAddress;

            if (!errors.Any())
            {
                //try to find an address with the same values (don't duplicate records)
                var address = _addressService.FindAddress(
                    (await _customerService.GetAddressesByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync())
                        .Id)).ToList(),
                    newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                    newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                    newAddress.Address1, newAddress.Address2, newAddress.City,
                    newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                    newAddress.CountryId, customAttributes);

                if (address == null)
                {
                    address = newAddress.FromDto<AddressModel>().ToEntity();
                    address.CustomAttributes = customAttributes;
                    address.CreatedOnUtc = DateTime.UtcNow;
                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    await _addressService.InsertAddressAsync(address);

                    await _customerService.InsertCustomerAddressAsync(await _workContext.GetCurrentCustomerAsync(),
                        address);
                }

                (await _workContext.GetCurrentCustomerAsync()).ShippingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

                return ApiResponseFactory.Success(
                    new NewShippingAddressResponse { RedirectToMethod = "ShippingMethod" });
            }

            //If we got this far, something failed, redisplay form
            var model = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart,
                newAddress.CountryId,
                overrideAttributesXml: customAttributes);
            return ApiResponseFactory.Success(new NewShippingAddressResponse
                { Model = model.ToDto<CheckoutShippingAddressModelDto>() });
        }

        /// <summary>
        ///     Prepare shipping method model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ShippingMethodResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ShippingMethod()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
            {
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(
                    await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute,
                    null, (await _storeContext.GetCurrentStoreAsync()).Id);
                return ApiResponseFactory.Success(new ShippingMethodResponse { RedirectToMethod = "PaymentMethod" });
            }

            //check if pickup point is selected on the shipping address step
            if (!_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
            {
                var selectedPickUpPoint = await _genericAttributeService
                    .GetAttributeAsync<PickupPoint>(await _workContext.GetCurrentCustomerAsync(),
                        NopCustomerDefaults.SelectedPickupPointAttribute,
                        (await _storeContext.GetCurrentStoreAsync()).Id);
                if (selectedPickUpPoint != null)
                    return ApiResponseFactory.Success(new ShippingMethodResponse
                        { RedirectToMethod = "PaymentMethod" });
            }

            //model
            var model = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart,
                await _customerService.GetCustomerShippingAddressAsync(await _workContext.GetCurrentCustomerAsync()));

            if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
                model.ShippingMethods.Count == 1)
            {
                //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedShippingOptionAttribute,
                    model.ShippingMethods.First().ShippingOption,
                    (await _storeContext.GetCurrentStoreAsync()).Id);

                return ApiResponseFactory.Success(new ShippingMethodResponse { RedirectToMethod = "PaymentMethod" });
            }

            return ApiResponseFactory.Success(new ShippingMethodResponse
                { Model = model.ToDto<CheckoutShippingMethodModelDto>() });
        }

        /// <summary>
        ///     Select shipping method
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CheckoutRedirectResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SelectShippingMethod([FromBody] IDictionary<string, string> form,
            [FromQuery] string shippingMethodSystemName, [FromQuery] [Required] string shippingOption)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart,
                store.Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(customer) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
            {
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute, null, store.Id);

                return ApiResponseFactory.Success(new CheckoutRedirectResponse { RedirectToMethod = "PaymentMethod" });
            }

            //pickup point
            if (_shippingSettings.AllowPickupInStore && _orderSettings.DisplayPickupInStoreOnShippingMethodPage)
            {
                var pickupInStore = ParsePickupInStore(form);
                if (pickupInStore)
                {
                    var pickupOption = await ParsePickupOptionAsync(form);
                    await SavePickupOptionAsync(pickupOption);

                    return ApiResponseFactory.Success(new CheckoutRedirectResponse
                        { RedirectToMethod = "PaymentMethod" });
                }

                //set value indicating that "pick up in store" option has not been chosen
                await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer,
                    NopCustomerDefaults.SelectedPickupPointAttribute, null, store.Id);
            }

            //parse selected method 
            if (string.IsNullOrEmpty(shippingOption))
                return ApiResponseFactory.Success(new CheckoutRedirectResponse { RedirectToMethod = "ShippingMethod" });

            var selectedShippingOptionName = shippingOption;
            var shippingRateComputationMethodSystemName = shippingMethodSystemName;

            if (string.IsNullOrEmpty(shippingMethodSystemName))
            {
                var splittedOption = shippingOption.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
                if (splittedOption.Length != 2)
                    return ApiResponseFactory.Success(
                        new CheckoutRedirectResponse { RedirectToMethod = "ShippingMethod" });

                selectedShippingOptionName = splittedOption[0];
                shippingRateComputationMethodSystemName = splittedOption[1];
            }

            //find it
            //performance optimization. try cache first
            var shippingOptions = await _genericAttributeService.GetAttributeAsync<List<ShippingOption>>(
                customer, NopCustomerDefaults.OfferedShippingOptionsAttribute, store.Id);

            if (shippingOptions == null || !shippingOptions.Any())
            {
                var address = await _customerService.GetCustomerShippingAddressAsync(customer);
                //not found? let's load them using shipping service
                shippingOptions = (await _shippingService.GetShippingOptionsAsync(cart, address, customer,
                    shippingRateComputationMethodSystemName, store.Id)).ShippingOptions.ToList();
            }
            else
            {
                //loaded cached results. let's filter result by a chosen shipping rate computation method
                shippingOptions = shippingOptions.Where(so =>
                        so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName,
                            StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
            }

            var shippingOpt = shippingOptions
                .Find(so => !string.IsNullOrEmpty(so.Name)
                            && so.Name.Equals(selectedShippingOptionName, StringComparison.InvariantCultureIgnoreCase));

            if (shippingOpt == null)
                return ApiResponseFactory.BadRequest(new CheckoutRedirectResponse { RedirectToMethod = "ShippingMethod" });

            //save
            await _genericAttributeService.SaveAttributeAsync(customer,
                NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOpt, store.Id);

            return ApiResponseFactory.Success(new CheckoutRedirectResponse { RedirectToMethod = "PaymentMethod" });
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GetPickupPointsResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> StorePickupPoints()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var getPickupPointsResponse = await _shippingService.GetPickupPointsAsync(
                customer.BillingAddressId ?? 0, customer, "Pickup.PickupInStore", store.Id);

            var pickupPoints = await getPickupPointsResponse.PickupPoints
                .Select(item => new PickupPointModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    ProviderSystemName = item.ProviderSystemName,
                    Address = item.Address,
                    City = item.City,
                    County = item.County,
                    StateAbbreviation = item.StateAbbreviation,
                    CountryCode = item.CountryCode,
                    ZipPostalCode = item.ZipPostalCode,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude,
                    PickupFee = item.PickupFee,
                    OpeningHours = item.OpeningHours,
                    DisplayOrder = item.DisplayOrder,
                    TransitDays = item.TransitDays
                }).ToListAsync();

            return ApiResponseFactory.Success(pickupPoints);
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PickupPoint), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SelectPickupOptionMethod(string storePickupPointId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var getPickupPointsResponse = await _shippingService.GetPickupPointsAsync(
                customer.BillingAddressId ?? 0,
                customer, "Pickup.PickupInStore", store.Id);

            var selectedPoint =
                getPickupPointsResponse.PickupPoints.FirstOrDefault(x => x.Id.Equals(storePickupPointId));

            if (selectedPoint == null)
                throw new Exception("Pickup point is not allowed");

            await SavePickupOptionAsync(selectedPoint);

            return ApiResponseFactory.Success(selectedPoint);

            async Task SavePickupOptionAsync(PickupPoint pickupPoint)
            {
                var name = !string.IsNullOrEmpty(pickupPoint.Name)
                    ? string.Format(await _localizationService.GetResourceAsync("Checkout.PickupPoints.Name"),
                        pickupPoint.Name)
                    : await _localizationService.GetResourceAsync("Checkout.PickupPoints.NullName");
                var pickUpInStoreShippingOption = new ShippingOption
                {
                    Name = name,
                    Rate = pickupPoint.PickupFee,
                    Description = pickupPoint.Description,
                    ShippingRateComputationMethodSystemName = pickupPoint.ProviderSystemName,
                    IsPickupInStore = true
                };

                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute, pickUpInStoreShippingOption, store.Id);

                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedPickupPointAttribute, pickupPoint, store.Id);
            }
        }


        /// <summary>
        ///     Prepare payment method model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CheckoutPaymentMethodModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PaymentMethod()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            //Check whether payment workflow is required
            //we ignore reward points during cart total calculation
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
            if (!isPaymentWorkflowRequired)
            {
                await _genericAttributeService.SaveAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, null,
                    (await _storeContext.GetCurrentStoreAsync()).Id);
                return ApiResponseFactory.Success(new PaymentMethodResponse { RedirectToMethod = "PaymentInfo" });
            }

            //filter by country
            var filterByCountryId = 0;
            if (_addressSettings.CountryEnabled)
                filterByCountryId =
                    (await _customerService.GetCustomerBillingAddressAsync(
                        await _workContext.GetCurrentCustomerAsync()))?.CountryId ?? 0;

            //model
            var paymentMethodModel =
                await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);

            if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
            {
                //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
                //so customer doesn't have to choose a payment method

                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute,
                    paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName,
                    (await _storeContext.GetCurrentStoreAsync()).Id);
                return ApiResponseFactory.Success(new PaymentMethodResponse
                {
                    RedirectToMethod = "PaymentInfo",
                    Model = paymentMethodModel.ToDto<CheckoutPaymentMethodModelDto>()
                });
            }

            return ApiResponseFactory.Success(new PaymentMethodResponse
                { Model = paymentMethodModel.ToDto<CheckoutPaymentMethodModelDto>() });
        }

        /// <summary>
        ///     Select payment method
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CheckoutRedirectResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SelectPaymentMethod([FromBody] CheckoutPaymentMethodModelDto model,
            [FromQuery] [Required] string paymentMethod)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            //reward points
            if (_rewardPointsSettings.Enabled)
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute, model.UseRewardPoints,
                    (await _storeContext.GetCurrentStoreAsync()).Id);

            //Check whether payment workflow is required
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
            if (!isPaymentWorkflowRequired)
            {
                await _genericAttributeService.SaveAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, null,
                    (await _storeContext.GetCurrentStoreAsync()).Id);
                return ApiResponseFactory.Success(new CheckoutRedirectResponse { RedirectToMethod = "PaymentInfo" });
            }

            //payment method 
            if (string.IsNullOrEmpty(paymentMethod) ||
                !await _paymentPluginManager.IsPluginActiveAsync(paymentMethod,
                    await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id))
                return ApiResponseFactory.Success(new CheckoutRedirectResponse { RedirectToMethod = "PaymentMethod" });

            //save
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.SelectedPaymentMethodAttribute, paymentMethod,
                (await _storeContext.GetCurrentStoreAsync()).Id);

            return ApiResponseFactory.Success(new CheckoutRedirectResponse { RedirectToMethod = "PaymentInfo" });
        }

        /// <summary>
        ///     Prepare payment info model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaymentInfoResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PaymentInfo()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart,
                currentStore.Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(currentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            //Check whether payment workflow is required
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
            if (!isPaymentWorkflowRequired)
            {
                var model = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
                return ApiResponseFactory.Success(new PaymentInfoResponse
                    { CheckoutConfirmModel = model.ToDto<CheckoutConfirmModelDto>() });
            }

            //load payment method
            var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(currentCustomer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, currentStore.Id);
            var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(paymentMethodSystemName,
                currentCustomer, currentStore.Id);
            if (paymentMethod == null)
                return ApiResponseFactory.NotFound("Payment method is not found.");

            //Check whether payment info should be skipped
            if (paymentMethod.SkipPaymentInfo ||
                (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection &&
                 _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
            {
                //skip payment info page
                var paymentInfo = new ProcessPaymentRequest();

                //paymentInfo save
                await SavePaymentInfoAsync(paymentInfo);

                var model = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
                return ApiResponseFactory.Success(new PaymentInfoResponse
                    { CheckoutConfirmModel = model.ToDto<CheckoutConfirmModelDto>() });
            }

            //model
            var paymentInfoModel = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);
            return ApiResponseFactory.Success(new PaymentInfoResponse
                { CheckoutPaymentInfoModel = paymentInfoModel.ToDto<CheckoutPaymentInfoModelDto>() });
        }

        /// <summary>
        ///     Enter payment Info
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CheckoutConfirmModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> EnterPaymentInfo([FromBody] IDictionary<string, string> form)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart,
                currentStore.Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(currentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            //Check whether payment workflow is required
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
            if (!isPaymentWorkflowRequired)
            {
                var model = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
                return ApiResponseFactory.Success(model.ToDto<CheckoutConfirmModelDto>());
            }

            //load payment method
            var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(currentCustomer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, currentStore.Id);
            var paymentMethod = await _paymentPluginManager
                .LoadPluginBySystemNameAsync(paymentMethodSystemName, currentCustomer, currentStore.Id);
            if (paymentMethod == null)
                return ApiResponseFactory.NotFound("Payment method is not found.");

            var warnings =
                await paymentMethod.ValidatePaymentFormAsync(
                    new FormCollection(form.ToDictionary(i => i.Key, i => new StringValues(i.Value))));

            var errors = new List<string>();
            errors.AddRange(warnings);

            if (!errors.Any())
            {
                //get payment info
                var paymentInfo =
                    await paymentMethod.GetPaymentInfoAsync(
                        new FormCollection(form.ToDictionary(i => i.Key, i => new StringValues(i.Value))));
                //set previous order GUID (if exists)
                await GenerateOrderGuidAsync(paymentInfo);

                //paymentInfo save
                await SavePaymentInfoAsync(paymentInfo);

                //model
                var model = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
                return ApiResponseFactory.Success(model.ToDto<CheckoutConfirmModelDto>());
            }

            return ApiResponseFactory.BadRequest(errors);
        }

        /// <summary>
        ///     Prepare confirm order model
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CheckoutConfirmModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Confirm()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            //model
            var model = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
            return ApiResponseFactory.Success(model.ToDto<CheckoutConfirmModelDto>());
        }

        /// <summary>
        ///     Confirm order
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ConfirmOrderResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConfirmOrder([FromQuery] bool processPayment = true,
            [FromQuery] string device = "Mobile", [FromQuery] bool dontRequireAddress=false)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (_frontendApiSettings.DontRequireAddress && dontRequireAddress)
            {
                if (currentCustomer.BillingAddressId is null)
                    await _fakeAddressService.SetBillingAddressAsync(currentCustomer);
                
                if (currentCustomer.ShippingAddressId is null)
                    await _fakeAddressService.SetShippingAddressAsync(currentCustomer);
                
            }

            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart,
                currentStore.Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(currentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            //model
            var model = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
            try
            {
                //prevent 2 orders being placed within an X seconds time frame
                if (!await IsMinimumOrderPlacementIntervalValidAsync(currentCustomer))
                    return ApiResponseFactory.BadRequest(
                        await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"));

                //place order
                var processPaymentRequest = await GetPaymentInfoAsync();

                await GenerateOrderGuidAsync(processPaymentRequest);
                processPaymentRequest.StoreId = currentStore.Id;
                processPaymentRequest.CustomerId = currentCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName =
                    await _genericAttributeService.GetAttributeAsync<string>(currentCustomer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute, currentStore.Id);

                await SavePaymentInfoAsync(processPaymentRequest);

                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    await ClearPaymentInfoAsync();
                    var placedOrder = placeOrderResult.PlacedOrder;
                    
                    var orderTotal = decimal.Round(placedOrder.OrderTotal, 2, MidpointRounding.ToZero);

                    if (!processPayment)
                        return ApiResponseFactory.Success(new ConfirmOrderResponse
                        {
                            RedirectToMethod = "Completed",
                            Id = placedOrder.Id,
                            OrderTotal = orderTotal
                        });

                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placedOrder
                    };
                    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = placedOrder.Id,
                        CreatedOnUtc = DateTime.UtcNow,
                        Note = $"Placed by {device}"
                    });

                    if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                        return ApiResponseFactory.Success(new
                        {
                            url = Response.Headers["Location"].FirstOrDefault(),
                            OrderTotal = orderTotal
                        });
                    //redirection or POST has been done in PostProcessPayment
                    // return Content(await _localizationService.GetResourceAsync("Checkout.RedirectMessage"));

                    return ApiResponseFactory.Success(new ConfirmOrderResponse
                        { 
                            RedirectToMethod = "Completed",
                            Id = placedOrder.Id,
                            OrderTotal = orderTotal
                        });
                }

                foreach (var error in placeOrderResult.Errors)
                    model.Warnings.Add(error);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc);
                await _logger.WarningAsync(exc.StackTrace, exc);
                model.Warnings.Add(exc.Message);
            }

            //If we got this far, something failed, redisplay form
            var confirmOrderResponse = new ConfirmOrderResponse { Model = model.ToDto<CheckoutConfirmModelDto>() };
            return confirmOrderResponse.Model.Warnings.Any()
                ? ApiResponseFactory.BadRequest(confirmOrderResponse, confirmOrderResponse.Model.Warnings.First())
                : ApiResponseFactory.Success(confirmOrderResponse);
        }
        

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SummeryModel), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PaymentSummery()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart,
                currentStore.Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            /* if (await _customerService.IsGuestAsync(currentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                 return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");*/

            //Check whether payment workflow is required
            /*var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
            if (!isPaymentWorkflowRequired)
            {
                return ApiResponseFactory.Success();
            }*/

            //load payment method
            var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(currentCustomer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, currentStore.Id);

            //filter by country
            var filterByCountryId = 0;
            if (_addressSettings.CountryEnabled)
                filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(
                    await _workContext.GetCurrentCustomerAsync()))?.CountryId ?? 0;

            //model
            var methodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);
            var paymentMethodModel = methodModel.PaymentMethods
                .FirstOrDefault(item => item.PaymentMethodSystemName == paymentMethodSystemName);

            var totalsModel = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, true);
            decimal sumDiscount = 0;
            sumDiscount += totalsModel.OrderTotalDiscount.ExtractFirstDecimal();

            foreach (var shoppingCartItem in cart)
            {
                sumDiscount += (await _shoppingCartService.GetSubTotalAsync(shoppingCartItem, true)).discountAmount;
            }

            if (totalsModel.GiftCards != null && totalsModel.GiftCards.Any())
                foreach (var card in totalsModel.GiftCards)
                    sumDiscount += card.Amount.ExtractFirstDecimal();

            
            sumDiscount += totalsModel.SubTotalDiscount.ExtractFirstDecimal();
            
            totalsModel.CustomProperties["TotalDiscount"] =
                await _priceFormatter.FormatPriceAsync(sumDiscount, true, false);
            totalsModel.CustomProperties["TotalDiscountValue"] = sumDiscount;

            totalsModel.SubTotal ??= "0";
            totalsModel.OrderTotal ??= "0";

            return ApiResponseFactory.Success(new SummeryModel
                { PaymentMethod = paymentMethodModel, TotalsModel = totalsModel });
        }

        #endregion

        #region Methods (one page checkout)

        /// <summary>
        ///     Prepare one page checkout model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(OnePageCheckoutModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> OnePageCheckout()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is not enabled.");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

            var model = await _checkoutModelFactory.PrepareOnePageCheckoutModelAsync(cart);
            return ApiResponseFactory.Success(model.ToDto<OnePageCheckoutModelDto>());
        }

        /// <summary>
        ///     Save billing (OPC)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NextStepResponse<CheckoutBillingAddressModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> OpcSaveBilling(
            [FromBody] BaseModelDtoRequest<CheckoutBillingAddressModelDto> request)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    return ApiResponseFactory.NotFound(
                        $"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                    ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (!cart.Any())
                    return ApiResponseFactory.BadRequest("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    return ApiResponseFactory.NotFound(
                        $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is not enabled.");

                if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                    !_orderSettings.AnonymousCheckoutAllowed)
                    return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

                _ = int.TryParse(request.Form["billing_address_id"], out var billingAddressId);

                if (billingAddressId > 0)
                {
                    //existing address
                    var address =
                        await _customerService.GetCustomerAddressAsync(
                            (await _workContext.GetCurrentCustomerAsync()).Id, billingAddressId);
                    if (address == null)
                        return ApiResponseFactory.BadRequest(
                            await _localizationService.GetResourceAsync("Checkout.Address.NotFound"));

                    (await _workContext.GetCurrentCustomerAsync()).BillingAddressId = address.Id;
                    await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());
                }
                else
                {
                    //new address
                    var newAddress = request.Model.BillingNewAddress;

                    //custom address attributes
                    var customAttributes = await ParseCustomAddressAttributesAsync(request.Form);
                    var customAttributeWarnings =
                        await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);

                    var errors = new List<string>();
                    errors.AddRange(customAttributeWarnings);

                    //validate model
                    if (errors.Any())
                    {
                        //model is not valid. redisplay the form with errors
                        var billingAddressModel = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart,
                            newAddress.CountryId,
                            overrideAttributesXml: customAttributes);
                        billingAddressModel.NewAddressPreselected = true;
                        return ApiResponseFactory.Success(new NextStepResponse<CheckoutBillingAddressModelDto>
                        {
                            UpdateSectionModel = new UpdateSectionJsonModelDto<CheckoutBillingAddressModelDto>
                            {
                                Name = "billing",
                                ViewName = "OpcBillingAddress",
                                Model = billingAddressModel.ToDto<CheckoutBillingAddressModelDto>()
                            },
                            WrongBillingAddress = true
                        });
                    }

                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress(
                        (await _customerService.GetAddressesByCustomerIdAsync(
                            (await _workContext.GetCurrentCustomerAsync()).Id)).ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes);

                    if (address == null)
                    {
                        //address is not found. let's create a new one
                        address = newAddress.FromDto<AddressModel>().ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        //some validation
                        if (address.CountryId == 0)
                            address.CountryId = null;

                        if (address.StateProvinceId == 0)
                            address.StateProvinceId = null;

                        await _addressService.InsertAddressAsync(address);

                        await _customerService.InsertCustomerAddressAsync(await _workContext.GetCurrentCustomerAsync(),
                            address);
                    }

                    (await _workContext.GetCurrentCustomerAsync()).BillingAddressId = address.Id;

                    await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());
                }

                if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                {
                    //shipping is required
                    var address =
                        await _customerService.GetCustomerBillingAddressAsync(
                            await _workContext.GetCurrentCustomerAsync());

                    //by default Shipping is available if the country is not specified
                    var shippingAllowed = !_addressSettings.CountryEnabled ||
                                          ((await _countryService.GetCountryByAddressAsync(address))?.AllowsShipping ??
                                           false);
                    if (_shippingSettings.ShipToSameAddress && request.Model.ShipToSameAddress && shippingAllowed)
                    {
                        //ship to the same address
                        (await _workContext.GetCurrentCustomerAsync()).ShippingAddressId = address.Id;
                        await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());
                        //reset selected shipping method (in case if "pick up in store" was selected)
                        await _genericAttributeService.SaveAttributeAsync<ShippingOption>(
                            await _workContext.GetCurrentCustomerAsync(),
                            NopCustomerDefaults.SelectedShippingOptionAttribute, null,
                            (await _storeContext.GetCurrentStoreAsync()).Id);
                        await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                            await _workContext.GetCurrentCustomerAsync(),
                            NopCustomerDefaults.SelectedPickupPointAttribute, null,
                            (await _storeContext.GetCurrentStoreAsync()).Id);
                        //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 
                        return await OpcLoadStepAfterShippingAddress(cart);
                    }

                    //do not ship to the same address
                    var shippingAddressModel =
                        await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart,
                            prePopulateNewAddressWithCustomerFields: true);

                    return ApiResponseFactory.Success(new NextStepResponse<CheckoutShippingAddressModelDto>
                    {
                        UpdateSectionModel = new UpdateSectionJsonModelDto<CheckoutShippingAddressModelDto>
                        {
                            Name = "shipping",
                            ViewName = "OpcShippingAddress",
                            Model = shippingAddressModel.ToDto<CheckoutShippingAddressModelDto>()
                        },
                        GotoSection = "shipping"
                    });
                }

                //shipping is not required
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(
                    await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute,
                    null, (await _storeContext.GetCurrentStoreAsync()).Id);

                //load next step
                return await OpcLoadStepAfterShippingMethod(cart);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return ApiResponseFactory.BadRequest(exc.Message);
            }
        }

        /// <summary>
        ///     Save shipping (OPC)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NextStepResponse<CheckoutShippingAddressModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> OpcSaveShipping(
            [FromBody] BaseModelDtoRequest<CheckoutShippingAddressModelDto> request)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    return ApiResponseFactory.NotFound(
                        $"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                    ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (!cart.Any())
                    return ApiResponseFactory.BadRequest("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    return ApiResponseFactory.NotFound(
                        $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is not enabled.");

                if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                    !_orderSettings.AnonymousCheckoutAllowed)
                    return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

                if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                    return ApiResponseFactory.BadRequest("Shipping is not required");

                //pickup point
                if (_shippingSettings.AllowPickupInStore && !_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
                {
                    var pickupInStore = ParsePickupInStore(request.Form);
                    if (pickupInStore)
                    {
                        var pickupOption = await ParsePickupOptionAsync(request.Form);
                        await SavePickupOptionAsync(pickupOption);

                        return await OpcLoadStepAfterShippingMethod(cart);
                    }

                    //set value indicating that "pick up in store" option has not been chosen
                    await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                        await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute,
                        null, (await _storeContext.GetCurrentStoreAsync()).Id);
                }

                _ = int.TryParse(request.Form["shipping_address_id"], out var shippingAddressId);

                if (shippingAddressId > 0)
                {
                    //existing address
                    var address =
                        await _customerService.GetCustomerAddressAsync(
                            (await _workContext.GetCurrentCustomerAsync()).Id, shippingAddressId);
                    if (address == null)
                        return ApiResponseFactory.BadRequest(
                            await _localizationService.GetResourceAsync("Checkout.Address.NotFound"));

                    (await _workContext.GetCurrentCustomerAsync()).ShippingAddressId = address.Id;
                    await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());
                }
                else
                {
                    //new address
                    var newAddress = request.Model.ShippingNewAddress.FromDto<AddressModel>();

                    //custom address attributes
                    var customAttributes = await ParseCustomAddressAttributesAsync(request.Form);
                    var customAttributeWarnings =
                        await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);

                    var errors = new List<string>();
                    errors.AddRange(customAttributeWarnings);

                    //validate model
                    if (errors.Any())
                    {
                        //model is not valid. redisplay the form with errors
                        var shippingAddressModel = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart,
                            newAddress.CountryId,
                            overrideAttributesXml: customAttributes);
                        shippingAddressModel.NewAddressPreselected = true;
                        return ApiResponseFactory.Success(new NextStepResponse<CheckoutShippingAddressModelDto>
                        {
                            UpdateSectionModel = new UpdateSectionJsonModelDto<CheckoutShippingAddressModelDto>
                            {
                                Name = "shipping",
                                ViewName = "OpcShippingAddress",
                                Model = shippingAddressModel.ToDto<CheckoutShippingAddressModelDto>()
                            }
                        });
                    }

                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress(
                        (await _customerService.GetAddressesByCustomerIdAsync(
                            (await _workContext.GetCurrentCustomerAsync()).Id)).ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes);

                    if (address == null)
                    {
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        await _addressService.InsertAddressAsync(address);

                        await _customerService.InsertCustomerAddressAsync(await _workContext.GetCurrentCustomerAsync(),
                            address);
                    }

                    (await _workContext.GetCurrentCustomerAsync()).ShippingAddressId = address.Id;

                    await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());
                }

                return await OpcLoadStepAfterShippingAddress(cart);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return ApiResponseFactory.BadRequest(exc.Message);
            }
        }

        /// <summary>
        ///     Save shipping method (OPC)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> OpcSaveShippingMethod([FromBody] IDictionary<string, string> form,
            [FromQuery] [Required] string shippingOption)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    return ApiResponseFactory.NotFound(
                        $"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                    ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (!cart.Any())
                    return ApiResponseFactory.BadRequest("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    return ApiResponseFactory.NotFound(
                        $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is not enabled.");

                if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                    !_orderSettings.AnonymousCheckoutAllowed)
                    return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

                if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                    return ApiResponseFactory.BadRequest("Shipping is not required");

                //pickup point
                if (_shippingSettings.AllowPickupInStore && _orderSettings.DisplayPickupInStoreOnShippingMethodPage)
                {
                    var pickupInStore = ParsePickupInStore(form);
                    if (pickupInStore)
                    {
                        var pickupOption = await ParsePickupOptionAsync(form);
                        await SavePickupOptionAsync(pickupOption);

                        return await OpcLoadStepAfterShippingMethod(cart);
                    }

                    //set value indicating that "pick up in store" option has not been chosen
                    await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                        await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute,
                        null, (await _storeContext.GetCurrentStoreAsync()).Id);
                }

                //parse selected method 
                if (string.IsNullOrEmpty(shippingOption))
                    return ApiResponseFactory.BadRequest("Selected shipping method can't be parsed");

                var splittedOption = shippingOption.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
                if (splittedOption.Length != 2)
                    return ApiResponseFactory.BadRequest("Selected shipping method can't be parsed");

                var selectedName = splittedOption[0];
                var shippingRateComputationMethodSystemName = splittedOption[1];

                //find it
                //performance optimization. try cache first
                var shippingOptions = await _genericAttributeService.GetAttributeAsync<List<ShippingOption>>(
                    await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.OfferedShippingOptionsAttribute,
                    (await _storeContext.GetCurrentStoreAsync()).Id);
                if (shippingOptions == null || !shippingOptions.Any())
                    //not found? let's load them using shipping service
                    shippingOptions = (await _shippingService.GetShippingOptionsAsync(cart,
                        await _customerService.GetCustomerShippingAddressAsync(
                            await _workContext.GetCurrentCustomerAsync()),
                        await _workContext.GetCurrentCustomerAsync(), shippingRateComputationMethodSystemName,
                        (await _storeContext.GetCurrentStoreAsync()).Id)).ShippingOptions.ToList();
                else
                    //loaded cached results. let's filter result by a chosen shipping rate computation method
                    shippingOptions = shippingOptions.Where(so =>
                            so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName,
                                StringComparison.InvariantCultureIgnoreCase))
                        .ToList();

                var shippingOpt = shippingOptions.Find(so => !string.IsNullOrEmpty(so.Name)
                                                             && so.Name.Equals(selectedName,
                                                                 StringComparison.InvariantCultureIgnoreCase));
                if (shippingOpt == null)
                    return ApiResponseFactory.BadRequest("Selected shipping method can't be loaded");

                //save
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOption,
                    (await _storeContext.GetCurrentStoreAsync()).Id);

                //load next step
                return await OpcLoadStepAfterShippingMethod(cart);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return ApiResponseFactory.BadRequest(exc.Message);
            }
        }

        /// <summary>
        ///     Save payment method (OPC)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NextStepResponse<CheckoutConfirmModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> OpcSavePaymentMethod([FromBody] CheckoutPaymentMethodModelDto model,
            [FromQuery] [Required] string paymentMethod)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    return ApiResponseFactory.NotFound(
                        $"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                    ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (!cart.Any())
                    return ApiResponseFactory.BadRequest("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    return ApiResponseFactory.NotFound(
                        $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is not enabled.");

                if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                    !_orderSettings.AnonymousCheckoutAllowed)
                    return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

                //payment method 
                if (string.IsNullOrEmpty(paymentMethod))
                    return ApiResponseFactory.BadRequest("Selected payment method can't be parsed");

                //reward points
                if (_rewardPointsSettings.Enabled)
                    await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                        NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute, model.UseRewardPoints,
                        (await _storeContext.GetCurrentStoreAsync()).Id);

                //Check whether payment workflow is required
                var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
                if (!isPaymentWorkflowRequired)
                {
                    //payment is not required
                    await _genericAttributeService.SaveAttributeAsync<string>(
                        await _workContext.GetCurrentCustomerAsync(),
                        NopCustomerDefaults.SelectedPaymentMethodAttribute, null,
                        (await _storeContext.GetCurrentStoreAsync()).Id);

                    var confirmOrderModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
                    return ApiResponseFactory.Success(new NextStepResponse<CheckoutConfirmModelDto>
                    {
                        UpdateSectionModel = new UpdateSectionJsonModelDto<CheckoutConfirmModelDto>
                        {
                            Name = "confirm-order",
                            ViewName = "OpcConfirmOrder",
                            Model = confirmOrderModel.ToDto<CheckoutConfirmModelDto>()
                        },
                        GotoSection = "confirm_order"
                    });
                }

                var paymentMethodInst = await _paymentPluginManager.LoadPluginBySystemNameAsync(paymentMethod,
                    await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
                if (!_paymentPluginManager.IsPluginActive(paymentMethodInst))
                    return ApiResponseFactory.BadRequest("Selected payment method can't be parsed");

                //save
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, paymentMethod,
                    (await _storeContext.GetCurrentStoreAsync()).Id);

                return await OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return ApiResponseFactory.BadRequest(exc.Message);
            }
        }

        /// <summary>
        ///     Save payment info (OPC)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NextStepResponse<CheckoutConfirmModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> OpcSavePaymentInfo([FromBody] IDictionary<string, string> form)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    return ApiResponseFactory.NotFound(
                        $"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

                var currentStore = await _storeContext.GetCurrentStoreAsync();
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();

                var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer,
                    ShoppingCartType.ShoppingCart, currentStore.Id);
                if (!cart.Any())
                    return ApiResponseFactory.BadRequest(new List<string> { "Your cart is empty" });

                if (!_orderSettings.OnePageCheckoutEnabled)
                    return ApiResponseFactory.NotFound(
                        $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is not enabled.");

                if (await _customerService.IsGuestAsync(currentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                    return ApiResponseFactory.BadRequest(new List<string> { "Anonymous checkout is not allowed" });

                var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(currentCustomer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, currentStore.Id);
                var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(paymentMethodSystemName,
                    currentCustomer, currentStore.Id);

                if (paymentMethod == null)
                    return ApiResponseFactory.BadRequest(new List<string> { "Payment method is not selected" });

                var formCollection = new FormCollection(form.ToDictionary(p => p.Key, p => new StringValues(p.Value)));

                var warnings = await paymentMethod.ValidatePaymentFormAsync(formCollection);

                var errors = new List<string>();
                errors.AddRange(warnings);

                if (!errors.Any())
                {
                    //get payment info
                    var paymentInfo = await paymentMethod.GetPaymentInfoAsync(formCollection);
                    //set previous order GUID (if exists)
                    await GenerateOrderGuidAsync(paymentInfo);

                    //paymentInfo save
                    await SavePaymentInfoAsync(paymentInfo);

                    var confirmOrderModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
                    return ApiResponseFactory.Success(new NextStepResponse<CheckoutConfirmModelDto>
                    {
                        UpdateSectionModel = new UpdateSectionJsonModelDto<CheckoutConfirmModelDto>
                        {
                            Name = "confirm-order",
                            ViewName = "OpcConfirmOrder",
                            Model = confirmOrderModel.ToDto<CheckoutConfirmModelDto>()
                        },
                        GotoSection = "confirm_order"
                    });
                }

                return ApiResponseFactory.BadRequest(errors);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return ApiResponseFactory.BadRequest(new List<string> { exc.Message });
            }
        }

        /// <summary>
        ///     Confirm order (OPC)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NextStepResponse<CheckoutConfirmModelDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status302Found)]
        public virtual async Task<IActionResult> OpcConfirmOrder()
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    return ApiResponseFactory.NotFound(
                        $"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

                var currentStore = await _storeContext.GetCurrentStoreAsync();
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();

                var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer,
                    ShoppingCartType.ShoppingCart, currentStore.Id);
                if (!cart.Any())
                    return ApiResponseFactory.BadRequest("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    return ApiResponseFactory.NotFound(
                        $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is not enabled.");

                if (await _customerService.IsGuestAsync(currentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                    return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

                //prevent 2 orders being placed within an X seconds time frame
                if (!await IsMinimumOrderPlacementIntervalValidAsync(currentCustomer))
                    return ApiResponseFactory.BadRequest(
                        await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"));

                //place order
                var processPaymentRequest = await GetPaymentInfoAsync();

                await GenerateOrderGuidAsync(processPaymentRequest);
                processPaymentRequest.StoreId = currentStore.Id;
                processPaymentRequest.CustomerId = currentCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName =
                    await _genericAttributeService.GetAttributeAsync<string>(currentCustomer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute, currentStore.Id);

                await SavePaymentInfoAsync(processPaymentRequest);

                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    await ClearPaymentInfoAsync();
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };

                    var paymentMethod = await _paymentPluginManager
                        .LoadPluginBySystemNameAsync(placeOrderResult.PlacedOrder.PaymentMethodSystemName,
                            currentCustomer, currentStore.Id);
                    if (paymentMethod == null)
                        //payment method could be null if order total is 0
                        //success
                        return NoContent();

                    if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                        //Redirection will not work because it's AJAX request.
                        //That's why we don't process it here (we redirect a user to another page where he'll be redirected)

                        //redirect
                        return Redirect($"{_webHelper.GetStoreLocation()}checkout/OpcCompleteRedirectionPayment");

                    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);
                    //success
                    return NoContent();
                }

                //error
                var confirmOrderModel = new CheckoutConfirmModel();
                foreach (var error in placeOrderResult.Errors)
                    confirmOrderModel.Warnings.Add(error);

                return ApiResponseFactory.Success(new NextStepResponse<CheckoutConfirmModelDto>
                {
                    UpdateSectionModel = new UpdateSectionJsonModelDto<CheckoutConfirmModelDto>
                    {
                        Name = "confirm-order",
                        ViewName = "OpcConfirmOrder",
                        Model = confirmOrderModel.ToDto<CheckoutConfirmModelDto>()
                    },
                    GotoSection = "confirm_order"
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return ApiResponseFactory.BadRequest(exc.Message);
            }
        }

        /// <summary>
        ///     Complete redirection payment (OPC)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CheckoutCompletedModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> OpcCompleteRedirectionPayment()
        {
            try
            {
                //validation
                if (!_orderSettings.OnePageCheckoutEnabled)
                    return ApiResponseFactory.NotFound(
                        $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is not enabled.");

                if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                    !_orderSettings.AnonymousCheckoutAllowed)
                    return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");

                //get the order
                var order = (await _orderService.SearchOrdersAsync(
                    (await _storeContext.GetCurrentStoreAsync()).Id,
                    customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1)).FirstOrDefault();
                if (order == null)
                    return ApiResponseFactory.NotFound("Order not found.");

                var paymentMethod = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(order.PaymentMethodSystemName,
                        await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
                if (paymentMethod == null)
                    return ApiResponseFactory.NotFound("Payment method not found.");

                if (paymentMethod.PaymentMethodType != PaymentMethodType.Redirection)
                    return ApiResponseFactory.BadRequest();

                //ensure that order has been just placed
                if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes > 3)
                    return ApiResponseFactory.BadRequest();

                //Redirection will not work on one page checkout page because it's AJAX request.
                //That's why we process it here
                var postProcessPaymentRequest = new PostProcessPaymentRequest
                {
                    Order = order
                };

                await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

                if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                    //redirection or POST has been done in PostProcessPayment
                    return Content(await _localizationService.GetResourceAsync("Checkout.RedirectMessage"));

                //if no redirection has been done (to a third-party payment page)
                //theoretically it's not possible
                var model = await _checkoutModelFactory.PrepareCheckoutCompletedModelAsync(order);
                return ApiResponseFactory.Success(model.ToDto<CheckoutCompletedModelDto>());
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return ApiResponseFactory.BadRequest(exc.Message);
            }
        }

        #endregion
    }
}