using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Orders;
using Nop.Plugin.Baramjk.FrontendApi.Exceptions.Vendors;
using Nop.Plugin.Baramjk.FrontendApi.Factories;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Helpers;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Baramjk.FrontendApi.Models.Orders;
using Nop.Plugin.Baramjk.FrontendApi.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Web.Factories;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class OrderController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPdfService _pdfService;
        private readonly IShipmentService _shipmentService;
        private readonly IWorkContext _workContext;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IWebApiOrderModelFactory _webApiOrderModelFactory;
        private readonly IDispatcherService _dispatcherService;
        private readonly ICustomerService _customerService;
        private readonly FrontendOrderService _frontendOrderService;

        #endregion

        #region Ctor

        public OrderController(IOrderModelFactory orderModelFactory,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPdfService pdfService,
            IShipmentService shipmentService,
            IWorkContext workContext,
            RewardPointsSettings rewardPointsSettings,
            IWebApiOrderModelFactory webApiOrderModelFactory,
            IDispatcherService dispatcherService,
            ICustomerService customerService,
            FrontendOrderService frontendOrderService)
        {
            _orderModelFactory = orderModelFactory;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentService = paymentService;
            _pdfService = pdfService;
            _shipmentService = shipmentService;
            _workContext = workContext;
            _rewardPointsSettings = rewardPointsSettings;
            _webApiOrderModelFactory = webApiOrderModelFactory;
            _dispatcherService = dispatcherService;
            _customerService = customerService;
            _frontendOrderService = frontendOrderService;
        }

        #endregion

        #region Methods

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CancelOrder(CancelOrderModel cancelOrderModel)
        {
            if (cancelOrderModel.OrderId <= 0)
                return ApiResponseFactory.BadRequest();

            var order = await _orderService.GetOrderByIdAsync(cancelOrderModel.OrderId);

            if (order == null)
                return ApiResponseFactory.NotFound($"Order Id={cancelOrderModel.OrderId} not found");

            if (order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return ApiResponseFactory.BadRequest("The order is deleted or belongs to another customer");

            await _orderProcessingService.CancelOrderAsync(order, cancelOrderModel.NotifyCustomer);
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = cancelOrderModel.Note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            return ApiResponseFactory.Success();
        }

        [HttpGet]
        [ProducesResponseType(typeof(CustomerOrderListModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> VendorOrders([FromQuery] bool includeSumOrderTotal = false)
        {
            try
            {
                var orderListModelDto = await _webApiOrderModelFactory.VendorOrdersSync(includeSumOrderTotal);
                return ApiResponseFactory.Success(orderListModelDto);
            }
            catch (VendorNotFoundException)
            {
                return ApiResponseFactory.BadRequest("you are not a vendor, please login as vendor and try again.");
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(CustomerOrderListModelDto), StatusCodes.Status200OK)]
        [HttpGet]
        [ProducesResponseType(typeof(CustomerOrderListModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CustomerOrders(bool withFirstProductPicture = false,
            int? orderStatus = null)
        {
            var customerOrderListModelDto = await _webApiOrderModelFactory
                .CustomerOrdersSync(withFirstProductPicture, orderStatus);

            return ApiResponseFactory.Success(customerOrderListModelDto);
        }

        //My account / Orders / Cancel recurring order
        [HttpPost]
        [ProducesResponseType(typeof(CustomerOrderListModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CancelRecurringPayment([FromBody] IDictionary<string, string> form)
        {
            //get recurring payment identifier
            var recurringPaymentId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("cancelRecurringPayment", StringComparison.InvariantCultureIgnoreCase))
                    recurringPaymentId = Convert.ToInt32(formValue["cancelRecurringPayment".Length..]);

            var recurringPayment = await _orderService.GetRecurringPaymentByIdAsync(recurringPaymentId);
            if (recurringPayment == null)
                return ApiResponseFactory.BadRequest();

            if (await _orderProcessingService.CanCancelRecurringPaymentAsync(
                    await _workContext.GetCurrentCustomerAsync(), recurringPayment))
            {
                var errors = await _orderProcessingService.CancelRecurringPaymentAsync(recurringPayment);

                var model = await _orderModelFactory.PrepareCustomerOrderListModelAsync();
                model.RecurringPaymentErrors = errors;

                return ApiResponseFactory.Success(model.ToDto<CustomerOrderListModelDto>());
            }

            return ApiResponseFactory.BadRequest();
        }

        //My account / Orders / Retry last recurring order
        [HttpPost]
        [ProducesResponseType(typeof(CustomerOrderListModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> RetryLastRecurringPayment([FromBody] IDictionary<string, string> form)
        {
            //get recurring payment identifier
            var recurringPaymentId = 0;
            if (!form.Keys.Any(formValue =>
                    formValue.StartsWith("retryLastPayment", StringComparison.InvariantCultureIgnoreCase) &&
                    int.TryParse(formValue[(formValue.IndexOf('_') + 1)..], out recurringPaymentId)))
                return ApiResponseFactory.BadRequest();

            var recurringPayment = await _orderService.GetRecurringPaymentByIdAsync(recurringPaymentId);
            if (recurringPayment == null)
                return ApiResponseFactory.BadRequest();

            if (!await _orderProcessingService.CanRetryLastRecurringPaymentAsync(
                    await _workContext.GetCurrentCustomerAsync(), recurringPayment))
                return ApiResponseFactory.BadRequest();

            var errors = await _orderProcessingService.ProcessNextRecurringPaymentAsync(recurringPayment);
            var model = await _orderModelFactory.PrepareCustomerOrderListModelAsync();
            model.RecurringPaymentErrors = errors.ToList();

            return ApiResponseFactory.Success(model.ToDto<CustomerOrderListModelDto>());
        }

        //My account / Reward points
        [HttpGet]
        [ProducesResponseType(typeof(CustomerRewardPointsModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> CustomerRewardPoints([FromQuery] int? pageNumber)
        {
            if (!_rewardPointsSettings.Enabled)
                return ApiResponseFactory.BadRequest();

            var model = await _orderModelFactory.PrepareCustomerRewardPointsAsync(pageNumber);

            return ApiResponseFactory.Success(model.ToDto<CustomerRewardPointsModelDto>());
        }

        //My account / Order details page
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(OrderDetailsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Details(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return ApiResponseFactory.NotFound($"Order Id = {orderId} not found");

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (order.Deleted ||
                (customer.Id != order.CustomerId && await _customerService.IsAdminAsync(customer) == false))
                return ApiResponseFactory.BadRequest("The order is deleted or belongs to another customer");

            var model = await _webApiOrderModelFactory.PrepareOrderDetailsModelAsync(order);
            await _dispatcherService.PublishAsync(BaramjkDispatcherDefaults.OrderDeliveryInfo, model);

            return ApiResponseFactory.Success(model);
        }

        [HttpGet("/FrontendApi/Order/ApiDetails/{orderId:int}")]
        [Authorize(true)]
        public virtual async Task<IActionResult> ApiDetails(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return ApiResponseFactory.NotFound($"Order Id = {orderId} not found");

            if (order.Deleted)
                return ApiResponseFactory.BadRequest("The order is deleted");

            var model = await _webApiOrderModelFactory.PrepareOrderDetailsModelAsync(order);

            return ApiResponseFactory.Success(model);
        }

        //My account / Order details page / PDF invoice
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetPdfInvoice(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return ApiResponseFactory.NotFound($"Order Id = {orderId} not found");

            if (order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return ApiResponseFactory.BadRequest("The order is deleted or belongs to another customer");

            var orders = new List<Order> { order };
            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _pdfService.PrintOrdersToPdfAsync(stream, orders,
                    (await _workContext.GetWorkingLanguageAsync()).Id);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, $"order_{order.Id}.pdf");
        }

        //My account / Order details page / re-order
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ReOrder(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return ApiResponseFactory.NotFound($"Order Id = {orderId} not found");

            if (order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return ApiResponseFactory.BadRequest("The order is deleted or belongs to another customer");

            var results = await _frontendOrderService.ReOrderAsync(order);
            var dto = await _webApiOrderModelFactory.PrepareReOrderDtoAsync(results);

            return ApiResponseFactory.Success(dto);
        }

        //My account / Order details page / Complete payment
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RePostPayment(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return ApiResponseFactory.NotFound($"Order Id = {orderId} not found");

            if (order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return ApiResponseFactory.BadRequest("The order is deleted or belongs to another customer");

            if (!await _paymentService.CanRePostProcessPaymentAsync(order))
                return ApiResponseFactory.BadRequest();

            var postProcessPaymentRequest = new PostProcessPaymentRequest
            {
                Order = order
            };
            await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

            return ApiResponseFactory.Success();
        }

        //My account / Order details page / Shipment details page
        [HttpGet("{shipmentId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ShipmentDetailsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ShipmentDetails(int shipmentId)
        {
            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);

            if (shipment == null)
                return ApiResponseFactory.NotFound($"Shipment Id = {shipmentId} not found");

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

            if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return ApiResponseFactory.BadRequest("The order is deleted or belongs to another customer");

            var model = await _orderModelFactory.PrepareShipmentDetailsModelAsync(shipment);

            return ApiResponseFactory.Success(model.ToDto<ShipmentDetailsModelDto>());
        }


        [HttpPost("/FrontendApi/Order/OrderTrack")]
        public async Task<IActionResult> OrderTrack([FromBody] OrderTrackRequest request)
        {
            if (request == null)
                return ApiResponseFactory.BadRequest("Invalid input");

            var result = await _frontendOrderService.TrackOrderAsync(request.Identifier, request.OrderId);

            if (!result.Success)
                return ApiResponseFactory.BadRequest(result.ErrorMessage);

            var orderDetails = await _webApiOrderModelFactory.PrepareOrderDetailsModelAsync(result.Order);
            return ApiResponseFactory.Success(orderDetails);
        }

        #endregion
    }
}