using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Extensions;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Checkout;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Checkout;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Factories;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Frontend
{
    public class CheckoutApiController : BaseBaramjkApiController
    {
        private readonly OrderSettings _orderSettings;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly PhotoPlatformShoppingCartService _photoPlatformShoppingCartService;
        private readonly ICustomerService _customerService;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly PaymentSettings _paymentSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentService _paymentService;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;



        public CheckoutApiController(OrderSettings orderSettings,
            IStoreContext storeContext,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            PhotoPlatformShoppingCartService photoPlatformShoppingCartService,
            ICheckoutModelFactory checkoutModelFactory,
            IOrderService orderService,
            ILocalizationService localizationService,
            IStaticCacheManager staticCacheManager,
            PaymentSettings paymentSettings,
            IGenericAttributeService genericAttributeService,
            IPaymentService paymentService,
            IWebHelper webHelper,
            ILogger logger,
            IOrderProcessingService orderProcessingService)
        {
            _orderSettings = orderSettings;
            _storeContext = storeContext;
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _photoPlatformShoppingCartService = photoPlatformShoppingCartService;
            _checkoutModelFactory = checkoutModelFactory;
            _orderService = orderService;
            _localizationService = localizationService;
            _staticCacheManager = staticCacheManager;
            _paymentSettings = paymentSettings;
            _genericAttributeService = genericAttributeService;
            _paymentService = paymentService;
            _webHelper = webHelper;
            _logger = logger;
            _orderProcessingService = orderProcessingService;

        }

        private async Task<bool> IsMinimumOrderPlacementIntervalValidAsync()
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

        private async Task<CacheKey> PreparePaymentInfoCacheKey()
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(DefaultValues.PaymentInfoCacheKey,
                await _storeContext.GetCurrentStoreAsync(),
                (await _workContext.GetCurrentCustomerAsync()).CustomerGuid);
            return key;
        }

        private async Task<ProcessPaymentRequest> GetPaymentInfoAsync()
        {
            var key = await PreparePaymentInfoCacheKey();
            return await _staticCacheManager.GetAsync(key, () => new ProcessPaymentRequest());
        }

        private async Task GenerateOrderGuidAsync(ProcessPaymentRequest processPaymentRequest)
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

        private async Task SavePaymentInfoAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var key = await PreparePaymentInfoCacheKey();
            await _staticCacheManager.SetAsync(key, processPaymentRequest);
        }

        private async Task ClearPaymentInfoAsync()
        {
            var key = await PreparePaymentInfoCacheKey();
            await _staticCacheManager.RemoveAsync(key);
        }

        [HttpPost("/FrontendApi/PhotoPlatform/Checkout/Confirm")]
        public async Task<IActionResult> ConfirmOrderAsync([FromBody] ConfirmOrderApiParams apiParams)
        {
            if (_orderSettings.CheckoutDisabled)
                return ApiResponseFactory.BadRequest($"the setting {nameof(_orderSettings.CheckoutDisabled)} is disabled");

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, currentStore.Id);
            if (shoppingCart.Any() == false)
                return ApiResponseFactory.BadRequest("Your cart is empty");

            if (_orderSettings.OnePageCheckoutEnabled)
                return ApiResponseFactory.BadRequest(
                    $"The setting {nameof(_orderSettings.OnePageCheckoutEnabled)} is true.");

            if (await _customerService.IsGuestAsync(currentCustomer) && !_orderSettings.AnonymousCheckoutAllowed)
                return ApiResponseFactory.BadRequest("Anonymous checkout is not allowed");


            var shoppingCartValidationResult = await _photoPlatformShoppingCartService.ValidateShoppingCartAsync();
            if (!shoppingCartValidationResult.IsSuccess)
            {
                return ApiResponseFactory.BadRequest(shoppingCartValidationResult.Message);
            }

            var model = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(shoppingCart);

            try
            {
                //prevent 2 orders being placed within an X seconds time frame
                if (!await IsMinimumOrderPlacementIntervalValidAsync())
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


                    if (string.IsNullOrEmpty(placedOrder.CheckoutAttributeDescription))
                    {
                        placedOrder.CheckoutAttributeDescription = DefaultValues.DefaultCheckoutAttributeDescription;
                    }

                    if (apiParams.ProcessPayment == false)
                    {
                        return ApiResponseFactory.Success(new ConfirmOrderResponse
                            { RedirectToMethod = "Completed", Id = placedOrder.Id });
                    }

                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placedOrder
                    };

                    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);


                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = placedOrder.Id,
                        CreatedOnUtc = DateTime.UtcNow,
                        Note = $"Placed by {apiParams.Device}"
                    });


                    if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                        return ApiResponseFactory.Success(new
                        {
                            url = Response.Headers["Location"].FirstOrDefault(),
                            placedOrder.Id
                        });
                    //redirection or POST has been done in PostProcessPayment
                    // return Content(await _localizationService.GetResourceAsync("Checkout.RedirectMessage"));


                    return ApiResponseFactory.Success(new ConfirmOrderResponse
                        { RedirectToMethod = "Completed", Id = placedOrder.Id });
                }

                foreach (var error in placeOrderResult.Errors)
                    model.Warnings.Add(error);
            }
            catch (Exception e)
            {
                await _logger.WarningAsync(e.Message, e);
                model.Warnings.Add(e.Message);
            }

            //If we got this far, something failed, redisplay form
            var confirmOrderResponse = new ConfirmOrderResponse
                { Model = model.Map<CheckoutConfirmModelDto>() };
            return confirmOrderResponse.Model.Warnings.Any()
                ? ApiResponseFactory.BadRequest(confirmOrderResponse, confirmOrderResponse.Model.Warnings.First())
                : ApiResponseFactory.Success(confirmOrderResponse);
        }
    }
}