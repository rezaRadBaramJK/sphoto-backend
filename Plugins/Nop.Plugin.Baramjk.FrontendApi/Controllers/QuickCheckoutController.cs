using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Customer;
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
using Nop.Web.Factories;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class QuickCheckoutController : BaseNopWebApiFrontendController
    {
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressService _addressService;
        private readonly AddressSettings _addressSettings;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly OrderSettings _orderSettings;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly PaymentSettings _paymentSettings;
        private readonly IProductService _productService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IShippingService _shippingService;
        private readonly ShippingSettings _shippingSettings;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;

        public QuickCheckoutController(AddressSettings addressSettings,
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
            ICustomerModelFactory customerModelFactory)
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
            _customerModelFactory = customerModelFactory;
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GetInfoModel), StatusCodes.Status200OK)]
        public virtual async Task<GetInfoModel> GetInfo()
        {
            var customerAddressListModelDto = await Addresses();
            var storePickupPoints = await StorePickupPoints();
            var shippingMethod = await ShippingMethod();
            var paymentMethods = await PaymentMethod();
            foreach (var address in customerAddressListModelDto.Addresses)
            {
                address.AvailableCountries = new List<SelectListItemDto>();
                address.AvailableStates = new List<SelectListItemDto>();
            }

            return new GetInfoModel
            {
                CustomerAddressListModelDto = customerAddressListModelDto,
                ShippingMethod = shippingMethod,
                StorePickupPoints = storePickupPoints,
                PaymentMethods = paymentMethods
            };
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CheckoutRedirectResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CheckOut([FromBody] CheckOutRequest model)
        {
            await SelectBillingAddress(model.BillingAddressId, model.ShipToSameAddress);
            await SelectShippingAddress(model.ShippingAddressId);
            await SelectShippingMethod(model.ShippingMethodSystemName, model.ShippingOption);
            await SelectPaymentMethod(model.UseRewardPoints, model.PaymentMethod);
            if (model.StorePickupPointId != "")
                await SelectPickupOptionMethod(model.StorePickupPointId);
            await PaymentInfo();
            var confirmOrderResponse = await ConfirmOrder();

            return ApiResponseFactory.Success(confirmOrderResponse);
        }

        private async Task<CustomerAddressListModelDto> Addresses()
        {
            var model = await _customerModelFactory.PrepareCustomerAddressListModelAsync();

            return model.ToDto<CustomerAddressListModelDto>();
        }

        private async Task<List<PickupPointModel>> StorePickupPoints()
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

            return pickupPoints;
        }

        private async Task<ShippingMethodResponse> ShippingMethod()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                throw new Exception($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                throw new Exception(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");

            if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
            {
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(
                    await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute,
                    null, (await _storeContext.GetCurrentStoreAsync()).Id);
                return new ShippingMethodResponse { RedirectToMethod = "PaymentMethod" };
            }

            //check if pickup point is selected on the shipping address step
            if (!_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
            {
                var selectedPickUpPoint = await _genericAttributeService
                    .GetAttributeAsync<PickupPoint>(await _workContext.GetCurrentCustomerAsync(),
                        NopCustomerDefaults.SelectedPickupPointAttribute,
                        (await _storeContext.GetCurrentStoreAsync()).Id);
                if (selectedPickUpPoint != null)
                    return new ShippingMethodResponse
                        { RedirectToMethod = "PaymentMethod" };
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

                return new ShippingMethodResponse { RedirectToMethod = "PaymentMethod" };
            }

            return new ShippingMethodResponse
                { Model = model.ToDto<CheckoutShippingMethodModelDto>() };
        }

        private async Task<PaymentMethodResponse> PaymentMethod()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                throw new Exception($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                throw new Exception($"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");

            //Check whether payment workflow is required
            //we ignore reward points during cart total calculation
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
            if (!isPaymentWorkflowRequired)
            {
                await _genericAttributeService.SaveAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, null,
                    (await _storeContext.GetCurrentStoreAsync()).Id);
                return new PaymentMethodResponse { RedirectToMethod = "PaymentInfo" };
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
                return new PaymentMethodResponse { RedirectToMethod = "PaymentInfo" };
            }

            return new PaymentMethodResponse
                { Model = paymentMethodModel.ToDto<CheckoutPaymentMethodModelDto>() };
        }

        private async Task<IActionResult> SelectBillingAddress(int addressId,
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

        private async Task<IActionResult> SelectShippingAddress(int addressId)
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

        private async Task<IActionResult> SelectShippingMethod(string shippingMethodSystemName, string shippingOption)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart,
                store.Id);
            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(customer) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");

            if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
            {
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute, null, store.Id);

                return ApiResponseFactory.Success(new CheckoutRedirectResponse { RedirectToMethod = "PaymentMethod" });
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
                return ApiResponseFactory.Success(new CheckoutRedirectResponse { RedirectToMethod = "ShippingMethod" });

            //save
            await _genericAttributeService.SaveAttributeAsync(customer,
                NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOpt, store.Id);

            return ApiResponseFactory.Success(new CheckoutRedirectResponse { RedirectToMethod = "PaymentMethod" });
        }

        private async Task<IActionResult> SelectPickupOptionMethod(string storePickupPointId)
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

        private async Task<IActionResult> SelectPaymentMethod(bool useRewardPoints, string paymentMethod)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");

            //reward points
            if (_rewardPointsSettings.Enabled)
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute, useRewardPoints,
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

        private async Task<IActionResult> PaymentInfo()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart,
                currentStore.Id);
            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(currentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");

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

        private async Task<ConfirmOrderResponse> ConfirmOrder([FromQuery] bool processPayment = true,
            [FromQuery] string device = "Mobile")
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                throw new Exception($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true.");

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart,
                currentStore.Id);
            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                throw new Exception(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(currentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");

            //model
            var model = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
            try
            {
                //prevent 2 orders being placed within an X seconds time frame
                if (!await IsMinimumOrderPlacementIntervalValidAsync(currentCustomer))
                    throw new Exception(
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

                    if (!processPayment)
                        return new ConfirmOrderResponse
                            { RedirectToMethod = "Completed", Id = placedOrder.Id };

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
                    {
                        /*  return  (new
                          {
                              url = Response.Headers["Location"].FirstOrDefault()
                          });*/
                        //redirection or POST has been done in PostProcessPayment
                        //  return Content(await _localizationService.GetResourceAsync("Checkout.RedirectMessage"));
                    }

                    return new ConfirmOrderResponse
                        { RedirectToMethod = "Completed", Id = placedOrder.Id };
                }

                foreach (var error in placeOrderResult.Errors)
                    model.Warnings.Add(error);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }

            //If we got this far, something failed, redisplay form
            return new ConfirmOrderResponse
                { Model = model.ToDto<CheckoutConfirmModelDto>() };
        }

        #region Utilities

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
    }

    public class CheckOutRequest
    {
        public int BillingAddressId { get; set; }
        public bool ShipToSameAddress { get; set; }
        public int ShippingAddressId { get; set; }
        public string ShippingMethodSystemName { get; set; }
        public string ShippingOption { get; set; }
        public string StorePickupPointId { get; set; }
        public bool UseRewardPoints { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class GetInfoModel
    {
        public CustomerAddressListModelDto CustomerAddressListModelDto { get; set; }
        public ShippingMethodResponse ShippingMethod { get; set; }
        public List<PickupPointModel> StorePickupPoints { get; set; }
        public PaymentMethodResponse PaymentMethods { get; set; }
    }
}