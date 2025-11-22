using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class PaymentMyFatoorahController : BasePaymentController
    {
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IWebHelper _webHelper;
        private readonly MyFatoorahSettings _myFatoorahPaymentSettings;
        private readonly MyFatoorahHttpClient _myFatoorahHttpClient;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly TransactionService _transactionService;
        private string _successRedirectUrl;
        private string _failRedirectUrl;

        public PaymentMyFatoorahController(
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            ILogger logger,
            INotificationService notificationService,
            IWebHelper webHelper,
            MyFatoorahSettings myFatoorahPaymentSettings,
            MyFatoorahHttpClient myFatoorahHttpClient, 
            IRepository<GenericAttribute> genericAttributeRepository,
            TransactionService transactionService)
        {
          
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _logger = logger;
            _notificationService = notificationService;
            _webHelper = webHelper;
            _myFatoorahPaymentSettings = myFatoorahPaymentSettings;
            _myFatoorahHttpClient = myFatoorahHttpClient;
            _genericAttributeRepository = genericAttributeRepository;
            _transactionService = transactionService;
            _successRedirectUrl =
                $"{_myFatoorahPaymentSettings.FrontendBase}{_myFatoorahPaymentSettings.SuccessFrontendCallback}";
            _failRedirectUrl =
                $"{_myFatoorahPaymentSettings.FrontendBase}{_myFatoorahPaymentSettings.FailedFrontendCallback}";
        }

        protected virtual async Task ProcessRecurringPayment(string invoiceId, PaymentStatus newPaymentStatus,
            string transactionId, string ipnInfo)
        {
            Guid orderNumberGuid;
            try
            {
                orderNumberGuid = new Guid(invoiceId);
            }
            catch
            {
                orderNumberGuid = Guid.Empty;
            }

            Order order = await _orderService.GetOrderByGuidAsync(orderNumberGuid);
            if (order == null)
            {
                await _logger.ErrorAsync("MyFatoorah. Order is not found", (Exception)new NopException(ipnInfo),
                    (Customer)null);
                return;
            }

            foreach (RecurringPayment rp in (IEnumerable<RecurringPayment>)(await
                         _orderService.SearchRecurringPaymentsAsync(0, 0, ((BaseEntity)order).Id, (OrderStatus?)null, 0,
                             int.MaxValue, false)))
            {
                if ((int)newPaymentStatus != 20 && (int)newPaymentStatus != 30)
                {
                    if ((int)newPaymentStatus == 50)
                    {
                        ProcessPaymentResult val = new ProcessPaymentResult();
                        val.Errors = new string[1] { "MyFatoorah. Recurring payment is " + "Voided".ToLower() + " ." };
                        val.RecurringPaymentFailed = true;
                        ProcessPaymentResult failedPaymentResult = val;
                        await _orderProcessingService.ProcessNextRecurringPaymentAsync(rp, failedPaymentResult);
                    }
                }
                else if (!(await _orderService.GetRecurringPaymentHistoryAsync(rp)).Any())
                {
                    await _orderService.InsertRecurringPaymentHistoryAsync(new RecurringPaymentHistory
                    {
                        RecurringPaymentId = ((BaseEntity)rp).Id,
                        OrderId = ((BaseEntity)order).Id,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }
                else
                {
                    ProcessPaymentResult processPaymentResult = new ProcessPaymentResult
                    {
                        NewPaymentStatus = newPaymentStatus
                    };
                    if ((int)newPaymentStatus == 20)
                    {
                        processPaymentResult.AuthorizationTransactionId = transactionId;
                    }
                    else
                    {
                        processPaymentResult.CaptureTransactionId = transactionId;
                    }

                    await _orderProcessingService.ProcessNextRecurringPaymentAsync(rp, processPaymentResult);
                }
            }

            await _logger.InformationAsync("MyFatoorah. Recurring info", (Exception)new NopException(ipnInfo),
                (Customer)null);
        }

        protected virtual async Task ProcessPayment(string orderNumber, string ipnInfo, PaymentStatus newPaymentStatus,
            decimal mcGross, string transactionId)
        {
            //IL_0027: Unknown result type (might be due to invalid IL or missing references)
            //IL_0028: Unknown result type (might be due to invalid IL or missing references)
            Guid orderNumberGuid;
            try
            {
                orderNumberGuid = new Guid(orderNumber);
            }
            catch
            {
                orderNumberGuid = Guid.Empty;
            }

            Order order = await _orderService.GetOrderByGuidAsync(orderNumberGuid);
            if (order == null)
            {
                await _logger.ErrorAsync("MyFatoorah. Order is not found", (Exception)new NopException(ipnInfo),
                    (Customer)null);
                return;
            }

            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = ((BaseEntity)order).Id,
                Note = ipnInfo,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            if (((int)newPaymentStatus == 20 || (int)newPaymentStatus == 30) &&
                !Math.Round(mcGross, 2).Equals(Math.Round(order.OrderTotal, 2)))
            {
                string errorStr =
                    $"MyFatoorah. Returned order total {mcGross} doesn't equal order total {order.OrderTotal}. Order# {((BaseEntity)order).Id}.";
                await _logger.ErrorAsync(errorStr, (Exception)null, (Customer)null);
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = ((BaseEntity)order).Id,
                    Note = errorStr,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                return;
            }

            if ((int)newPaymentStatus <= 30)
            {
                if ((int)newPaymentStatus != 20)
                {
                    if ((int)newPaymentStatus == 30 && _orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        order.AuthorizationTransactionId = transactionId;
                        await _orderService.UpdateOrderAsync(order);
                        await _orderProcessingService.MarkOrderAsPaidAsync(order);
                    }
                }
                else if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
                {
                    await _orderProcessingService.MarkAsAuthorizedAsync(order);
                }

                return;
            }

            if ((int)newPaymentStatus != 40)
            {
                if ((int)newPaymentStatus == 50 && _orderProcessingService.CanVoidOffline(order))
                {
                    await _orderProcessingService.VoidOfflineAsync(order);
                }

                return;
            }

            decimal totalToRefund = Math.Abs(mcGross);
            if (totalToRefund > 0m && Math.Round(totalToRefund, 2).Equals(Math.Round(order.OrderTotal, 2)))
            {
                if (_orderProcessingService.CanRefundOffline(order))
                {
                    await _orderProcessingService.RefundOfflineAsync(order);
                }
            }
            else if (_orderProcessingService.CanPartiallyRefundOffline(order, totalToRefund))
            {
                await _orderProcessingService.PartiallyRefundOfflineAsync(order, totalToRefund);
            }
        }
        
        [HttpGet("/Plugins/PaymentMyFatoorah/CancelOrder")]
        [HttpGet("/Plugins/PaymentMyFatoorah/PDTHandler")]
        public async Task<IActionResult> PDTHandler()
        {
            try
            {
                var (isValid, order) = await ValidateOrderResponseAsync();
                _successRedirectUrl = _successRedirectUrl.Replace("{orderId}", order.Id.ToString());
                _failRedirectUrl = _failRedirectUrl.Replace("{orderId}", order.Id.ToString());
                
                if (isValid == false)
                {
                    if (_myFatoorahPaymentSettings.EnableRedirect)
                    {
                        return Redirect(_failRedirectUrl);

                    }
                    return RedirectToAction("Homepage");
                }

                if (_orderProcessingService.CanMarkOrderAsPaid(order) == false)
                {
                    if (_myFatoorahPaymentSettings.EnableRedirect)
                        return Redirect(_failRedirectUrl);
                    
                    return RedirectToRoute("CheckoutCompleted", new
                    {
                        orderId = order.Id
                    });
                }
                await _orderService.UpdateOrderAsync(order);
                await _orderProcessingService.MarkOrderAsPaidAsync(order);
                await _transactionService.MarkTransactionAsPaidAsync(order.Id);
                
                if (_myFatoorahPaymentSettings.EnableRedirect)
                    return Redirect(_successRedirectUrl);
                
                return RedirectToRoute("CheckoutCompleted", new
                {
                    orderId = order.Id
                });


            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Error while getting response from MyFatoorah", ex, (Customer)null);
                
                if (_myFatoorahPaymentSettings.EnableRedirect)
                    return Redirect(_failRedirectUrl);
                
                return RedirectToAction("Homepage");
            }
        }
        
        private async Task<(bool isValid, Order order)> ValidateOrderResponseAsync()
        {
            var idResponse = _webHelper.QueryString<string>("id");
           
            var orderResponse = _myFatoorahHttpClient.GetPaymentStatusAsync(idResponse).Result;
            if (orderResponse == null)
            {
                return (isValid: false, order: null);
            }
            var genericAttr = await _genericAttributeRepository.Table
                .Where(x => x.Key == "MyFatoorahInvoiceId" && x.Value == Convert.ToString(orderResponse.Data.InvoiceId.ToString())).FirstOrDefaultAsync();
            var order = await _orderService.GetOrderByIdAsync(genericAttr.EntityId);
            if (order == null)
                return (isValid: false, order: null);

            var paidTransaction = orderResponse.Data.InvoiceTransactions.SingleOrDefault(x => x.TransactionStatus == "Succss");
            if (paidTransaction != null)
            {
                order.AuthorizationTransactionId = paidTransaction.TransactionId;
                return (isValid: true, order);
            }
            
            var failedTransaction = orderResponse.Data.InvoiceTransactions.LastOrDefault(x => x.TransactionStatus != "Succss");
            orderResponse.Message = failedTransaction?.Error;

            var errorMessage = string.Concat(str2: Url.RouteUrl("ReOrder", new
                {
                    orderId = order.Id
                }), str0: orderResponse.MessageSummary,
                str1: ", Your order has been cancelled due to failure payment. Please try to <a href='",
                str3: "'>Re-Order</a>");
            
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = "MyFatoorah payment failed. " + orderResponse.MessageSummary +$" order invoice id : {genericAttr.Value}",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            
            _notificationService.ErrorNotification(errorMessage);

            return (isValid: false, order);

        }
    }
}