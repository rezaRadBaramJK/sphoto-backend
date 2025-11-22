using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.OrderPaymentLink.Services;
using Nop.Services.Orders;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using PaymentStatus = Nop.Core.Domain.Payments.PaymentStatus;

namespace Nop.Plugin.Baramjk.OrderPaymentLink.Controllers
{
    public class OrderPaymentLinkApiController : BasePluginController
    {
        private readonly InvoiceService _invoiceService;
        private readonly IWebHelper _webHelper; 
        private readonly IOrderService _orderService;

        public OrderPaymentLinkApiController(
            InvoiceService invoiceService,
            IWebHelper webHelper,
            IOrderService orderService)
        {
            _invoiceService = invoiceService;
            _webHelper = webHelper;
            _orderService = orderService;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpGet("/OrderPaymentLink/CreateInvoice")]
        public async Task<IActionResult> CreateInvoice([FromQuery] int orderId, [FromQuery] bool force,
            [FromQuery] bool sendEmail = true)
        {
            if (force == false)
            {
                var paymentStatus = await _invoiceService.GetOrderPaymentStatus(orderId);
                if (paymentStatus == PaymentStatus.Paid)
                    return ApiResponseFactory.Success("Already paid", "Already paid");
            }

            var invoiceUrl = await _invoiceService.CreateInvoice(orderId, sendEmail);
            
            return string.IsNullOrEmpty(invoiceUrl) 
                ? ApiResponseFactory.InternalServerError("An error occured while creating invoice. Please contact with admin.") 
                : ApiResponseFactory.Success(invoiceUrl);
        }

        [HttpGet("/OrderPaymentLink/CallBack")]
        public async Task<IActionResult> CallBack(string guid)
        {
            var translation = await _invoiceService.CallBack(guid);
            var url = "/orderdetails/" + translation.ConsumerEntityId;

            return Redirect(url);
        }

        [HttpGet("/api-frontend/OrderPaymentLink/CreateInvoice")]
        [HttpPost("/FrontendApi/OrderPaymentLink/CreateInvoice")]
        public async Task<IActionResult> CreateInvoiceApi(
            [FromQuery] int orderId,
            [FromQuery] bool force,
            [FromQuery] bool sendEmail = false,
            [FromQuery] bool sendSms = false)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return ApiResponseFactory.NotFound("Order not found");
            
            var paymentStatus = await _invoiceService.GetOrderPaymentStatus(orderId);
            var isSendPaymentLink = await _invoiceService.IsSendPaymentLinkAsync(orderId);

            var result = new
            {
                PaymentStatus = paymentStatus,
                ISentBefore = isSendPaymentLink
            };

            if (force == false)
            {
                if (paymentStatus == PaymentStatus.Paid)
                    return ApiResponseFactory.BadRequest(result, "Already paid");

                if (isSendPaymentLink)
                    return ApiResponseFactory.BadRequest(result, "Payment link already sent");
            }

            var invoiceUrl = await _invoiceService.CreateInvoice(orderId, sendEmail, sendSms);
            return string.IsNullOrEmpty(invoiceUrl) 
                ? ApiResponseFactory.InternalServerError("An error occured while creating invoice. Please contact with admin.") 
                : ApiResponseFactory.Success(invoiceUrl);
        }

        [HttpGet("/api-frontend/OrderPaymentLink/PaymentLinks")]
        [HttpGet("/FrontendApi/OrderPaymentLink/PaymentLinks")]
        public async Task<IActionResult> GetPaymentLinkItemsAsync([FromQuery] int orderId)
        {
            var paymentLinkItems = await _invoiceService.GetPaymentLinkItemsAsync(orderId);
            return ApiResponseFactory.Success(paymentLinkItems);
        }

        [HttpGet("/api-frontend/OrderPaymentLink/Report")]
        [HttpGet("/FrontendApi/OrderPaymentLink/Report")]
        public async Task<IActionResult> Report([FromQuery] ReportRequest request)
        {
            /* if (!await _permissionService.AuthorizeAsync(PermissionRecords.ReportOrderPaymentLink))
                 return ApiResponseFactory.Unauthorized("Need ReportOrderPaymentLink access");
 */
            //   request.From ??= DateTime.Now;
            //  request.To ??= DateTime.Now;

            var reportItems = await _invoiceService.GetReportAsync(request);
            return ApiResponseFactory.Success(reportItems);
        }


        [HttpPost("/api-frontend/OrderPaymentLink/GetOrdersPaymentLinkItems")]
        [HttpPost("/FrontendApi/OrderPaymentLink/GetOrdersPaymentLinkItems")]
        public async Task<IActionResult> GetOrdersPaymentLinkItemsAsync([FromQuery] string orderIds)
        {
            var orderLinks = new List<object>();

            var ids = orderIds.Split(",");
            foreach (var id in ids)
            {
                var orderId = int.Parse(id);
                var paymentLinkItems = await _invoiceService.GetPaymentLinkItemsAsync(orderId);
                orderLinks.Add(new
                {
                    OrderId = orderId,
                    paymentLinkItems.Count,
                    Items = paymentLinkItems
                });
            }

            return ApiResponseFactory.Success(orderLinks);
        }
        
        [HttpPost("/api-frontend/OrderPaymentLink/CreatePaymentUrl")]
        [HttpPost("/FrontendApi/OrderPaymentLink/CreatePaymentUrl")]
        public async Task<IActionResult> CreatePaymentUrl(
            [FromQuery] int orderId, 
            [FromQuery] bool force,
            [FromQuery] bool sendEmail = true,
            [FromQuery] bool sendSms = false)
        {
            var paymentStatus = await _invoiceService.GetOrderPaymentStatus(orderId);
            var isSendPaymentLink = await _invoiceService.IsSendPaymentLinkAsync(orderId);

            var result = new
            {
                PaymentStatus = paymentStatus,
                ISentBefore = isSendPaymentLink
            };

            if (force == false)
            {
                if (paymentStatus == PaymentStatus.Paid)
                    return ApiResponseFactory.BadRequest(result, "Already paid");

                if (isSendPaymentLink)
                    return ApiResponseFactory.BadRequest(result, "Payment link already sent");
            }

            var transaction = await _invoiceService.CreatePayment(orderId, sendEmail, sendSms);
            var baseUrl = _webHelper.GetStoreLocation(true);
            var paymentUrl = baseUrl + "OrderPaymentLink/PaymentLink/" + transaction.Guid;
            
            return ApiResponseFactory.Success(paymentUrl, "ok");
        }
        
        [HttpGet("/OrderPaymentLink/PaymentLink/{paymentGuid}")]
        public async Task<IActionResult> GetPaymentLinkByGuid([FromRoute] string paymentGuid)
        {
            var result = await _invoiceService.GetPaymentUrlByPaymentGuid(paymentGuid);
            if (result.Success)
            {
                return Redirect(result.PaymentUrl);
            }
            else
            {
                return ApiResponseFactory.BadRequest(result.ErrorMessage);
            }
        }
    }
}