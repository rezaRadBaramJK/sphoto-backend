using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.GetPaymentStatus;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services.Verify.Models;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Services.Verify
{
    public class CheckoutVerifyPaymentService : ICheckoutVerifyPaymentService
    {
        private readonly ILogger _logger;
        private readonly IMyFatoorahPaymentClient _myFatoorahPaymentClient;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IWorkContext _workContext;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;

        public CheckoutVerifyPaymentService(ILogger logger, IMyFatoorahPaymentClient myFatoorahPaymentClient,
            IOrderService orderService, IOrderProcessingService orderProcessingService,
            IPriceCalculationService priceCalculationService, IWorkContext workContext,
            IRepository<GenericAttribute> genericAttributeRepository)
        {
            _logger = logger;
            _myFatoorahPaymentClient = myFatoorahPaymentClient;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _priceCalculationService = priceCalculationService;
            _workContext = workContext;
            _genericAttributeRepository = genericAttributeRepository;
        }

        public async Task<CheckoutVerifyPaymentResponse> VerifyAsync(GetPaymentStatusRequest paymentStatusRequest)
        {
            var response = await _myFatoorahPaymentClient.GetPaymentStatusAsync(paymentStatusRequest);
            if (!response.IsSuccess)
            {
                var msg = response.GetFullMessage("Error connecting to My Fatoorah");
                await _logger.WarningAsync(msg);
                throw new BadGatewayBusinessException(msg);
            }

            if (!int.TryParse(response.Data.CustomerReference, out var orderId))
            {
                await _logger.WarningAsync("Error parsing order id");
                throw new BadGatewayBusinessException("Error parsing order id");
            }

            var genericAttr = await _genericAttributeRepository.Table
                .Where(x => x.Key == "MyFatoorahInvoiceId" &&
                            x.Value == Convert.ToString(paymentStatusRequest.InvoiceId)
                )
                .FirstOrDefaultAsync();
            Order order = default;
            if (genericAttr != default)
            {
                order = await _orderService.GetOrderByIdAsync(genericAttr.EntityId);
            }

            if (order == null)
            {
                order = await _orderService.GetOrderByIdAsync(orderId);
            }

            if (order == null)
            {
                await _logger.WarningAsync($"Order with id {genericAttr.EntityId} not found");
                throw new NotFoundBusinessException("Order not found");
            }


            var roundedOrderTotal = await _priceCalculationService.RoundPriceAsync(order.OrderTotal,
                (await _workContext.GetWorkingCurrencyAsync()));
            if (response.Data.InvoiceValue < roundedOrderTotal)
            {
                await _logger.WarningAsync(
                    $"Paid amount is not equal to the orders amount. orderTotal = {order.OrderTotal}, InvoiceValue= {response.Data.InvoiceValue}");
                throw new ValidateBusinessException("Paid amount is not equal to the orders amount");
            }

            if (response.Data.InvoiceStatus != "Paid")
            {
                var verifyPaymentResponseModel = new CheckoutVerifyPaymentResponse
                {
                    OrderId = order.Id,
                    IsVerified = false
                };
                return verifyPaymentResponseModel;
            }

            if (_orderProcessingService.CanMarkOrderAsPaid(order))
                await _orderProcessingService.MarkOrderAsPaidAsync(order);

            var paymentResponseModel = new CheckoutVerifyPaymentResponse
            {
                OrderId = order.Id,
                IsVerified = true
            };
            return paymentResponseModel;
        }
    }
}