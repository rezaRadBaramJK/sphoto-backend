using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Baramjk.Framework.Events.Orders;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public interface IPushEventHandlerService
    {
        // Task HandleEventAsync(OrderPlacedEvent eventMessageEntity);
        Task HandleEventAsync(OrderNote eventMessageEntity);
        Task HandleEventAsync(OrderPaymentStatusEvent eventMessageEntity);
        Task HandleEventAsync(OrderStatusEvent eventMessageEntity);
        Task HandleEventAsync(OrderShippingStatusEvent eventMessageEntity);
        // Task HandleEventAsync(OrderPaidEvent eventMessage);
    }

    public class SmsEventHandler : IPushEventHandlerService
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly SmsTemplateSetting _smsTemplate;
        private  ISmsProvider _smsProvider;
        private readonly ILogger _logger;
        private readonly ISmsServiceFactory _smsServiceFactory;

        public SmsEventHandler(ICustomerService customerService, IOrderService orderService,
            SmsTemplateSetting smsTemplate, ILogger logger, ISmsServiceFactory smsServiceFactory)
        {
            _customerService = customerService;
            _orderService = orderService;
            _smsTemplate = smsTemplate;

            _logger = logger;
            _smsServiceFactory = smsServiceFactory;
        }

        private ISmsProvider SmsProvider => _smsProvider ??= _smsServiceFactory.GetService();

        // public async Task HandleEventAsync(OrderPlacedEvent eventMessageEntity)
        // {
        //     await _logger.InformationAsync(
        //         $"event OrderPlacedEvent : {JsonConvert.SerializeObject(eventMessageEntity)}");
        //     if (_smsTemplate.IsOrderPlacedEnabled)
        //     {
        //         var textTemplate = new Builder(_logger)
        //             .WithCustomer(await _customerService.GetCustomerByIdAsync(eventMessageEntity.Order.CustomerId))
        //             .WithOrder(eventMessageEntity.Order)
        //             .Build();
        //         var message = textTemplate.Tokenize(_smsTemplate.OrderPlaced);
        //         await _smsProvider.SetSetting(SmsProviderMode.Promotional);
        //         await _smsProvider.SendMessageAsync(textTemplate.Customer.Username, message);
        //     }
        //     else
        //     {
        //         await _logger.WarningAsync("IsOrderPlacedEnabled is disabled");
        //     }
        // }

        public async Task HandleEventAsync(OrderNote eventMessageEntity)
        {
            await _logger.InformationAsync($"event OrderNote : {JsonConvert.SerializeObject(eventMessageEntity)}");
            if (!eventMessageEntity.DisplayToCustomer)
            {
                await _logger.WarningAsync("OrderNote is not DisplayToCustomer");
            }

            if (_smsTemplate.IsOrderNoteEnabled)
            {
                var order = await _orderService.GetOrderByIdAsync(eventMessageEntity.OrderId);
                var textTemplate = new Builder(_logger)
                    .WithCustomer(await _customerService.GetCustomerByIdAsync(order.CustomerId))
                    .WithOrder(order)
                    .WithOrderNote(eventMessageEntity.Note)
                    .Build();
                var message = textTemplate.Tokenize(_smsTemplate.OrderNote);
                await SmsProvider.SetSetting(SmsProviderMode.Promotional);
                await SmsProvider.SendMessageAsync(textTemplate.Customer.Username, message);
            }
            else
            {
                await _logger.WarningAsync("IsOrderNoteEnabled is disabled");
            }
        }

        private string GetOrderStatusTemplate(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Pending:
                    return _smsTemplate.OrderStatusPending;
                case OrderStatus.Processing:
                    return _smsTemplate.OrderStatusProcessing;
                case OrderStatus.Complete:
                    return _smsTemplate.OrderStatusComplete;
                case OrderStatus.Cancelled:
                    return _smsTemplate.OrderStatusCancelled;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        private string GetOrderPaymentStatusTemplate(PaymentStatus paymentStatus)
        {
            switch (paymentStatus)
            {
                case PaymentStatus.Pending:
                    return _smsTemplate.OrderPaymentStatusPending;
                case PaymentStatus.Authorized:
                    return _smsTemplate.OrderPaymentStatusAuthorized;
                case PaymentStatus.Paid:
                    return _smsTemplate.OrderPaymentStatusPaid;
                case PaymentStatus.PartiallyRefunded:
                    return _smsTemplate.OrderPaymentStatusPartiallyRefunded;
                case PaymentStatus.Refunded:
                    return _smsTemplate.OrderPaymentStatusRefunded;
                case PaymentStatus.Voided:
                    return _smsTemplate.OrderPaymentStatusVoided;

                default:
                    throw new ArgumentOutOfRangeException(nameof(paymentStatus), paymentStatus, null);
            }
        }

        private bool GetOrderPaymentStatusActiveHandlerStatus(PaymentStatus paymentStatus)
        {
            switch (paymentStatus)
            {
                case PaymentStatus.Pending:
                    return _smsTemplate.IsOrderPaymentStatusPendingEnabled;
                case PaymentStatus.Authorized:
                    return _smsTemplate.IsOrderPaymentStatusAuthorizedEnabled;
                case PaymentStatus.Paid:
                    return _smsTemplate.IsOrderPaymentStatusPaidEnabled;
                case PaymentStatus.PartiallyRefunded:
                    return _smsTemplate.IsOrderPaymentStatusPartiallyRefundedEnabled;
                case PaymentStatus.Refunded:
                    return _smsTemplate.IsOrderPaymentStatusRefundedEnabled;
                case PaymentStatus.Voided:
                    return _smsTemplate.IsOrderPaymentStatusVoidedEnabled;
                default:
                    throw new ArgumentOutOfRangeException(nameof(paymentStatus), paymentStatus, null);
            }
        }

        private bool GetOrderStatusActiveHandlerStatus(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Pending:
                    return _smsTemplate.IsOrderStatusPendingEnabled;
                case OrderStatus.Processing:
                    return _smsTemplate.IsOrderStatusProcessingEnabled;
                case OrderStatus.Complete:
                    return _smsTemplate.IsOrderStatusCompleteEnabled;
                case OrderStatus.Cancelled:
                    return _smsTemplate.IsOrderStatusCancelledEnabled;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        private string GetOrderShippingStatus(ShippingStatus shippingStatus)
        {
            switch (shippingStatus)
            {
                case ShippingStatus.ShippingNotRequired:
                    return _smsTemplate.OrderShippingNotRequiredStatus;
                case ShippingStatus.NotYetShipped:
                    return _smsTemplate.OrderShippingNotYetShippedStatus;
                case ShippingStatus.PartiallyShipped:
                    return _smsTemplate.OrderShippingPartiallyShippedStatus;
                case ShippingStatus.Shipped:
                    return _smsTemplate.OrderShippingShippedStatus;
                case ShippingStatus.Delivered:
                    return _smsTemplate.OrderShippingDeliveredStatus;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shippingStatus), shippingStatus, null);
            }
        }

        private bool GetOrderShippingStatusActiveHandlerStatus(ShippingStatus shippingStatus)
        {
            switch (shippingStatus)
            {
                case ShippingStatus.ShippingNotRequired:
                    return _smsTemplate.IsOrderShippingNotRequiredStatusEnabled;
                case ShippingStatus.NotYetShipped:
                    return _smsTemplate.IsOrderShippingNotYetShippedStatusEnabled;
                case ShippingStatus.PartiallyShipped:
                    return _smsTemplate.IsOrderShippingPartiallyShippedStatusEnabled;
                case ShippingStatus.Shipped:
                    return _smsTemplate.IsOrderShippingShippedStatusEnabled;
                case ShippingStatus.Delivered:
                    return _smsTemplate.IsOrderShippingDeliveredStatusEnabled;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shippingStatus), shippingStatus, null);
            }
        }

        public async Task HandleEventAsync(OrderStatusEvent eventMessageEntity)
        {
            await _logger.InformationAsync(
                $"event OrderStatusEvent : {JsonConvert.SerializeObject(eventMessageEntity)}");

            if (GetOrderStatusActiveHandlerStatus(eventMessageEntity.Entity.OrderStatus))
            {
                var textTemplate = new Builder(_logger)
                    .WithCustomer(await _customerService.GetCustomerByIdAsync(eventMessageEntity.Entity.CustomerId))
                    .WithOrder(eventMessageEntity.Entity)
                    .Build();
                var message = textTemplate.Tokenize(GetOrderStatusTemplate(eventMessageEntity.Entity.OrderStatus));
                await SmsProvider.SetSetting(SmsProviderMode.Promotional);
                await SmsProvider.SendMessageAsync(textTemplate.Customer.Username, message);
                return;
            }

            await _logger.WarningAsync(
                $"OrderStatusEvent is disable for : {eventMessageEntity.Entity.OrderStatus}");
        }

        public async Task HandleEventAsync(OrderPaymentStatusEvent eventMessageEntity)
        {
            await _logger.InformationAsync(
                $"event OrderPaymentStatusEvent : {JsonConvert.SerializeObject(eventMessageEntity)}");

            if (GetOrderPaymentStatusActiveHandlerStatus(eventMessageEntity.Entity.PaymentStatus))
            {
                var textTemplate = new Builder(_logger)
                    .WithCustomer(await _customerService.GetCustomerByIdAsync(eventMessageEntity.Entity.CustomerId))
                    .WithOrder(eventMessageEntity.Entity)
                    .Build();
                var message = textTemplate.Tokenize(GetOrderStatusTemplate(eventMessageEntity.Entity.OrderStatus));
                await SmsProvider.SetSetting(SmsProviderMode.Promotional);
                await SmsProvider.SendMessageAsync(textTemplate.Customer.Username, message);
                return;
            }

            await _logger.WarningAsync(
                $"OrderPaymentStatusEvent is disable for : {eventMessageEntity.Entity.OrderStatus}");
        }


        public async Task HandleEventAsync(OrderShippingStatusEvent eventMessageEntity)
        {
            await _logger.InformationAsync(
                $"event OrderShippingStatusEvent : {JsonConvert.SerializeObject(eventMessageEntity)}");

            if (GetOrderShippingStatusActiveHandlerStatus(eventMessageEntity.Entity.ShippingStatus))
            {
                var textTemplate = new Builder(_logger)
                    .WithCustomer(await _customerService.GetCustomerByIdAsync(eventMessageEntity.Entity.CustomerId))
                    .WithOrder(eventMessageEntity.Entity)
                    .Build();
                var message = textTemplate.Tokenize(GetOrderShippingStatus(eventMessageEntity.Entity.ShippingStatus));
                await SmsProvider.SetSetting(SmsProviderMode.Promotional);
                await SmsProvider.SendMessageAsync(textTemplate.Customer.Username, message);
                return;
            }

            await _logger.WarningAsync(
                $"OrderShippingStatusEvent is disable for : {eventMessageEntity.Entity.ShippingStatus}");
        }

        // public async Task HandleEventAsync(OrderPaidEvent eventMessageEntity)
        // {
        //     await _logger.InformationAsync($"event OrderPaidEvent : {JsonConvert.SerializeObject(eventMessageEntity)}");
        //
        //     if (_smsTemplate.IsOrderPlacedEnabled)
        //     {
        //         var textTemplate = new Builder(_logger)
        //             .WithCustomer(await _customerService.GetCustomerByIdAsync(eventMessageEntity.Order.CustomerId))
        //             .WithOrder(eventMessageEntity.Order)
        //             .Build();
        //         var message = textTemplate.Tokenize(_smsTemplate.OrderPlaced);
        //         await _smsProvider.SetSetting(SmsProviderMode.Promotional);
        //         await _smsProvider.SendMessageAsync(textTemplate.Customer.Username, message);
        //     }
        //     else
        //     {
        //         await _logger.WarningAsync("IsOrderPlacedEnabled is disabled");
        //     }
        // }
    }
}