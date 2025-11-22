using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Extensions;
using Nop.Plugin.Baramjk.PhotoPlatform.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Cashier.Order;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ShoppingCart;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Checkout;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Services;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Factories;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Backend
{
    public class CashierApiController : BaseBaramjkApiController
    {
        private readonly EventService _eventService;
        private readonly IWorkContext _workContext;
        private readonly EventFactory _eventFactory;
        private readonly IOrderService _orderService;
        private readonly PhotoPlatformOrderFactory _photoPlatformOrderFactory;
        private readonly ReservationItemService _reservationItemService;
        private readonly PhotoPlatformOrderService _photoPlatformOrderService;
        private readonly OrderSettings _orderSettings;
        private readonly ILogger _logger;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly PhotoPlatformShoppingCartService _photoPlatformShoppingCartService;
        private readonly CashierEventService _cashierEventService;
        private readonly IStoreContext _storeContext;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly PaymentSettings _paymentSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly CashierBalanceService _cashierBalanceService;

        public CashierApiController(EventService eventService,
            IWorkContext workContext,
            EventFactory eventFactory,
            IOrderService orderService,
            PhotoPlatformOrderFactory photoPlatformOrderFactory,
            ReservationItemService reservationItemService,
            PhotoPlatformOrderService photoPlatformOrderService,
            OrderSettings orderSettings,
            ILogger logger,
            IShoppingCartService shoppingCartService,
            PhotoPlatformShoppingCartService photoPlatformShoppingCartService,
            CashierEventService cashierEventService,
            IStoreContext storeContext,
            ICheckoutModelFactory checkoutModelFactory,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IStaticCacheManager staticCacheManager,
            PaymentSettings paymentSettings,
            IGenericAttributeService genericAttributeService,
            CashierBalanceService cashierBalanceService)
        {
            _eventService = eventService;
            _workContext = workContext;
            _eventFactory = eventFactory;
            _orderService = orderService;
            _photoPlatformOrderFactory = photoPlatformOrderFactory;
            _reservationItemService = reservationItemService;
            _photoPlatformOrderService = photoPlatformOrderService;
            _orderSettings = orderSettings;
            _logger = logger;
            _shoppingCartService = shoppingCartService;
            _photoPlatformShoppingCartService = photoPlatformShoppingCartService;
            _cashierEventService = cashierEventService;
            _storeContext = storeContext;
            _checkoutModelFactory = checkoutModelFactory;
            _localizationService = localizationService;
            _orderProcessingService = orderProcessingService;
            _staticCacheManager = staticCacheManager;
            _paymentSettings = paymentSettings;
            _genericAttributeService = genericAttributeService;
            _cashierBalanceService = cashierBalanceService;
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

        private async Task<BaseServiceResult> IsOrderValidToRefund(Order order)
        {
            if (order == null || order.Deleted)
                return new BaseServiceResult("Order not found");


            if (order.OrderStatusId == (int)OrderStatus.Cancelled && order.PaymentStatusId == (int)PaymentStatus.Refunded)
                return new BaseServiceResult("Order is already Refunded.");


            if (order.OrderStatusId != (int)OrderStatus.Complete || order.PaymentStatusId != (int)PaymentStatus.Paid)
                return new BaseServiceResult("Can not refund an unpaid order.");

            var customer = await _workContext.GetCurrentCustomerAsync();

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

            var isCashierNotPermittedToRefund =
                await _cashierEventService.HasNotPermittedItemsToRefundAsync(orderItems.Select(oi => oi.ProductId).ToArray(), customer.Id);

            if (isCashierNotPermittedToRefund)
                return new BaseServiceResult("Cashier is not permitted to refund.");

            return new BaseServiceResult();
        }

        [AuthorizeApi(PermissionProvider.CashierName)]
        [HttpGet("/BackendApi/PhotoPlatform/Cashier/Event/")]
        public async Task<IActionResult> GetEventsAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var events = await _eventService.GetCashierEventsAsync(customer.Id);

            var result = await _eventFactory.PrepareCashierEventBriefDtosAsync(events);

            return ApiResponseFactory.Success(result);
        }


        [AuthorizeApi(PermissionProvider.CashierName)]
        [HttpGet("/BackendApi/PhotoPlatform/Cashier/Event/{id:int}")]
        public async Task<IActionResult> GetEventByIdAsync(int id)
        {
            if (id < 1)
            {
                return ApiResponseFactory.BadRequest("Provided id is invalid");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            var eventDetails = await _eventService.GetEventFullDetailsAsync(id, customer.Id);
            if (eventDetails == null)
            {
                return ApiResponseFactory.BadRequest("Event not found");
            }

            var result = await _eventFactory.PrepareCashierEventAsync(eventDetails, customer);

            return result == null ? ApiResponseFactory.BadRequest("Event not found") : ApiResponseFactory.Success(result);
        }


        [AuthorizeApi(PermissionProvider.CashierName)]
        [HttpGet("/BackendApi/PhotoPlatform/Cashier/CloseAccount/{eventId:int}")]
        public async Task<IActionResult> CloseAccountAsync(int eventId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (eventId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid eventId");
            }

            var cashierEvent = await _cashierEventService.GetByCashierIdAndEventIdAsync(customer.Id, eventId);

            if (cashierEvent == null)
            {
                return ApiResponseFactory.BadRequest("Event associated with this cashier was not found");
            }

            cashierEvent.Active = false;
            await _cashierEventService.UpdateAsync(cashierEvent);

            //todo: what is the report?


            return ApiResponseFactory.Success();
        }

        [AuthorizeApi(PermissionProvider.CashierName)]
        [HttpGet("/BackendApi/PhotoPlatform/Cashier/Order/{orderId:int}/RefundCheck")]
        public async Task<IActionResult> CheckOrderRefundAbility(int orderId)
        {
            if (orderId <= 0)
                return ApiResponseFactory.BadRequest("Invalid order id.");

            var order = await _orderService.GetOrderByIdAsync(orderId);


            var validationResult = await IsOrderValidToRefund(order);

            if (validationResult.IsSuccess == false)
            {
                return ApiResponseFactory.BadRequest(validationResult.Message);
            }

            var reservationWithDetails = await _reservationItemService.GetReservationDetailsByOrderIdAsync(order.Id);

            var result = await _photoPlatformOrderFactory.PrepareCashierOrderDetailDtoAsync(order, reservationWithDetails);


            return ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.CashierName)]
        [HttpPost("/BackendApi/PhotoPlatform/Cashier/Order/{orderId:int}/Refund")]
        public async Task<IActionResult> RefundOrderAsync(int orderId, [FromQuery] CashierRefundMethod refundMethod)
        {
            if (orderId <= 0)
                return ApiResponseFactory.BadRequest("Invalid order id.");

            var order = await _orderService.GetOrderByIdAsync(orderId);

            var validationResult = await IsOrderValidToRefund(order);

            if (validationResult.IsSuccess == false)
            {
                return ApiResponseFactory.BadRequest(validationResult.Message);
            }

            var result = await _photoPlatformOrderService.RefundOrderFromCashierAsync(order, refundMethod);

            return result.IsSuccess == false ? ApiResponseFactory.BadRequest(result.Message) : ApiResponseFactory.Success();
        }


        [AuthorizeApi(PermissionProvider.CashierName)]
        [HttpGet("/BackendApi/PhotoPlatform/Cashier/Order/{orderGuid:guid}/Details")]
        public async Task<IActionResult> GetOrderDetails(Guid orderGuid)
        {
            var order = await _orderService.GetOrderByGuidAsync(orderGuid);
            if (order == null)
            {
                return ApiResponseFactory.BadRequest("Order not found");
            }

            if (order.Deleted)
            {
                return ApiResponseFactory.Unauthorized("Order is not valid");
            }

            var reservationWithDetails = await _reservationItemService.GetReservationDetailsByOrderIdAsync(order.Id);

            var result = await _photoPlatformOrderFactory.PrepareCashierOrderDetailDtoAsync(order, reservationWithDetails,
                await _workContext.GetCurrentCustomerAsync());

            return ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.CashierName)]
        [HttpGet("/BackendApi/PhotoPlatform/Cashier/Order/{orderId:int}/Details")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            if (orderId < 1)
                return ApiResponseFactory.BadRequest("Invalid order id.");

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return ApiResponseFactory.BadRequest("Order not found");
            }

            if (order.Deleted)
            {
                return ApiResponseFactory.Unauthorized("Order is not valid");
            }

            var reservationWithDetails = await _reservationItemService.GetReservationDetailsByOrderIdAsync(order.Id);

            var result = await _photoPlatformOrderFactory.PrepareCashierOrderDetailDtoAsync(order, reservationWithDetails,
                await _workContext.GetCurrentCustomerAsync());

            return ApiResponseFactory.Success(result);
        }

        private CashierPaymentMethods PaymentMethodMapper(string paymentMethod)
        {
            if (paymentMethod == null)
                return CashierPaymentMethods.Cash;

            if (paymentMethod.ToLower().Contains("cash"))
            {
                return CashierPaymentMethods.Cash;
            }

            if (paymentMethod.ToLower().Contains("myfatoorah") || paymentMethod.ToLower().Contains("knet"))
            {
                return CashierPaymentMethods.KNet;
            }

            return CashierPaymentMethods.Cash;
        }

        [AuthorizeApi(PermissionProvider.CashierName)]
        [HttpPost("/BackendApi/PhotoPlatform/Cashier/Checkout/Confirm")]
        public async Task<IActionResult> ConfirmOrderAsync([FromBody] CashierConfirmOrderApiParams apiParams)
        {
            var mappedPaymentMethod = PaymentMethodMapper(apiParams.PaymentMethod);

            if (string.IsNullOrEmpty(apiParams.CustomerPhoneNumber))
                return ApiResponseFactory.BadRequest("You must provide a phone number.");

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


            var shoppingCartValidationResult = await _photoPlatformShoppingCartService.ValidateShoppingCartAsync(true);
            if (shoppingCartValidationResult.IsSuccess == false)
            {
                return ApiResponseFactory.BadRequest(shoppingCartValidationResult.Message);
            }


            //! custom logic for cashier - calculate the price of shopping cart items and check if it is more than the cashier fund balance for that event or not

            // var toBeInsertedCashierBalanceTransactionRequests = new List<CashierBalanceTransactionRequest>();

            foreach (var shoppingCartItem in shoppingCart)
            {
                var cashierEvent = await _cashierEventService.GetByCashierIdAndEventIdAsync(currentCustomer.Id, shoppingCartItem.ProductId);

                if (cashierEvent == null)
                {
                    return ApiResponseFactory.BadRequest($"This cashier is not associated with Event with ID of: {shoppingCartItem.ProductId}");
                }

                if (cashierEvent.Active == false)
                {
                    return ApiResponseFactory.BadRequest($"This cashier is not active for Event with ID of: {shoppingCartItem.ProductId}");
                }

                // var price = await _photoPlatformShoppingCartService.CalculateShoppingCartPriceAsync(new[] { shoppingCartItem.Id });
                //!2 due to new logic for cashier balance, we skip these balance checking
                // var cashierBalance = await _cashierBalanceService.GetBalanceAsync(cashierEvent.Id);
                // if (mappedPaymentMethod == CashierPaymentMethods.Cash && cashierBalance < price)
                //     return ApiResponseFactory.BadRequest(
                //         $"Your fund balance is less than the total amount. For event with Id of: {cashierEvent.EventId}, increase your fund balance first.");
                
                // if (mappedPaymentMethod == CashierPaymentMethods.Cash)
                // {
                //     var request = new CashierBalanceTransactionRequest
                //     {
                //         CashierEventId = cashierEvent.Id,
                //         Amount = price,
                //         Type = CashierBalanceHistoryType.SubmittedTicketByCash,
                //         Note = "Cashier Submitted Ticket"
                //     };
                //     if (await _cashierBalanceService.CanPerformAsync(request) == false)
                //     {
                //         return ApiResponseFactory.BadRequest("Failed to perform deduction on cashier fund balance.");
                //     }
                //
                //
                //     toBeInsertedCashierBalanceTransactionRequests.Add(request);
                // }
            }

            //!

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

                //!custom logic for cashier, provided payment method in api Params will be saved in advance 

                processPaymentRequest.PaymentMethodSystemName = DefaultValues.DefaultPaymentMethodSystemName;

                //!

                await SavePaymentInfoAsync(processPaymentRequest);


                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);

                if (placeOrderResult.Success)
                {
                    await ClearPaymentInfoAsync();
                    var placedOrder = placeOrderResult.PlacedOrder;


                    //! custom logic for cashier - update opening fund balance for each cashier event after marking order as paid
                    var cleanedCustomerPhoneNumber = apiParams.CustomerPhoneNumber.Replace("+", "");


                    await _genericAttributeService.SaveAttributeAsync(placedOrder, DefaultValues.CustomerPhoneForCashierOrderAttributeKey,
                        cleanedCustomerPhoneNumber);


                    await _genericAttributeService.SaveAttributeAsync(placedOrder, DefaultValues.OrderPlacedByCashierAttributeKey,
                        currentCustomer.Id);

                    await _photoPlatformOrderService.MarkAsPaidAsync(placeOrderResult.PlacedOrder, currentCustomer, mappedPaymentMethod);

                    
                    //!2 due to new logic for cashier balance, we skip these transactions
                    // foreach (var request in toBeInsertedCashierBalanceTransactionRequests)
                    // {
                    //     await _cashierBalanceService.PerformAsync(request);
                    // }


                    //!
                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = placedOrder.Id,
                        CreatedOnUtc = DateTime.UtcNow,
                        Note = $"Placed by Cashier: {currentCustomer.Email}"
                    });


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


        [AuthorizeApi(PermissionProvider.CashierName)]
        [HttpGet("/BackendApi/PhotoPlatform/Cashier/Order/")]
        public async Task<IActionResult> GetCashierOrdersAsync(
            [FromQuery] string phoneNumber = null,
            [FromQuery] int eventId = 0,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = int.MaxValue
        )
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid Page Number or Page Size");
            }

            if (string.IsNullOrEmpty(phoneNumber) == false)
                phoneNumber = phoneNumber.Replace("+", "").Trim();

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var reservationsWithDetails =
                await _reservationItemService.GetCustomerOrdersReservationsAsync(currentCustomer.Id, phoneNumber, eventId, pageNumber - 1, pageSize);
            var result = await _photoPlatformOrderFactory.PrepareCashierOrderListAsync(reservationsWithDetails);

            return ApiResponseFactory.Success(result);
        }


        [AuthorizeApi(PermissionProvider.CashierName)]
        [HttpGet("/BackendApi/PhotoPlatform/Cashier/Order/Printed")]
        public async Task<IActionResult> SaveOrderPrintedAsync([FromQuery] int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return ApiResponseFactory.BadRequest("Order was not found");

            var a = await _genericAttributeService.GetAttributeAsync<int>(order, DefaultValues.OrderPrintCountAttributeKey);

            await _genericAttributeService.SaveAttributeAsync(order, DefaultValues.OrderPrintCountAttributeKey, a += 1);

            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                CreatedOnUtc = DateTime.UtcNow,
                Note = $"Order printed, printed times: {a}"
            });

            return ApiResponseFactory.Success();
        }


        [HttpPost("/BackendApi/PhotoPlatform/Cashier/ShoppingCart")]
        public async Task<IActionResult> AddToCartAsync([FromBody] SubmitShoppingCartItemsApiModel model)
        {
            if (model.Items.Select(i => i.EventId).Distinct().Count() > 1)
                return ApiResponseFactory.BadRequest("You cannot add more than one event");

            var result = await _photoPlatformShoppingCartService.ProcessAddingItemToCartAsync(model, true);
            return result.IsSuccess ? ApiResponseFactory.Success() : ApiResponseFactory.BadRequest(result.Message);
        }

        [HttpPut("/BackendApi/PhotoPlatform/Cashier/ShoppingCart")]
        public async Task<IActionResult> UpdateCartAsync([FromBody] SubmitShoppingCartItemsApiModel model)
        {
            if (model.Items.Select(i => i.EventId).Distinct().Count() > 1)
                return ApiResponseFactory.BadRequest("You cannot add more than one event");

            var result = await _photoPlatformShoppingCartService.ProcessUpdatingShoppingCartAsync(model, true);
            return result.IsSuccess ? ApiResponseFactory.Success() : ApiResponseFactory.BadRequest(result.Message);
        }


        [HttpGet("/BackendApi/PhotoPlatform/Cashier/ShoppingCart/Validate")]
        public async Task<IActionResult> ValidateCartAsync()
        {
            var result = await _photoPlatformShoppingCartService.ValidateShoppingCartAsync(true);
            return result.IsSuccess ? ApiResponseFactory.Success(result.IsSuccess) : ApiResponseFactory.BadRequest(result.Message);
        }
    }
}