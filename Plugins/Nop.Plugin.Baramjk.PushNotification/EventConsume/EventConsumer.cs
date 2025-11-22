using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Plugin.Baramjk.Framework.Events.Orders;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification.Delegates;
using Nop.Plugin.Baramjk.PushNotification.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.PushNotification.EventConsume
{
    public class EventConsumer :
        IConsumer<EntityInsertedEvent<OrderNote>>,
        IConsumer<OrderPaymentStatusEvent>,
        IConsumer<OrderStatusEvent>,
        IConsumer<OrderShippingStatusEvent>
    {
        private readonly EventPushService _eventPushService;
        private readonly IPushEventHandlerService _handlerService;
        private readonly SmsTemplateSetting _smsTemplate;
        private readonly EventNotificationConfigService _eventNotificationConfigService;
        private readonly IWhatsAppProvider _whatsAppProvider;
        private readonly ICustomerService _customerService;
        private readonly PushNotificationCustomerService _pushNotificationCustomerService;
        private readonly IAddressService _addressService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;

        public EventConsumer(
            EventPushService eventPushService,
            IPushEventHandlerService handlerService,
            ISettingService settingService,
            EventNotificationConfigService eventNotificationConfigService,
            WhatsAppProviderResolver whatsAppProviderResolver, 
            ICustomerService customerService,
            PushNotificationCustomerService pushNotificationCustomerService,
            IAddressService addressService,
            ILogger logger, IOrderService orderService)
        {
            _eventPushService = eventPushService;
            _handlerService = handlerService;
            _eventNotificationConfigService = eventNotificationConfigService;
            _customerService = customerService;
            _pushNotificationCustomerService = pushNotificationCustomerService;
            _addressService = addressService;
            _logger = logger;
            _orderService = orderService;
            _whatsAppProvider = whatsAppProviderResolver.Invoke();
            _smsTemplate = Task.Run(() => settingService.LoadSettingAsync<SmsTemplateSetting>()).Result;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<OrderNote> eventMessage)
        {
            var eventConfig = await _eventNotificationConfigService.GetAsync(DefaultValue.OrderNoteInsertedEventKey, string.Empty);
            
            if(eventConfig.UseFirebase)
                await _eventPushService.HandleEventAsync(eventMessage.Entity);
            
            if (eventMessage.Entity.DisplayToCustomer &&
                _smsTemplate.IsOrderNoteEnabled &&
                !string.IsNullOrEmpty(_smsTemplate.OrderNote) &&
                eventConfig.UseSms)
            {
                await _handlerService.HandleEventAsync(eventMessage.Entity);
            }

            if (eventConfig.UseWhatsApp)
            {
                var order = await _orderService.GetOrderByIdAsync(eventMessage.Entity.OrderId);
                await SendWhatsAppStatusChangedAsync(order, eventConfig.TemplateName);
            }
                
        }
        
        private async Task SendWhatsAppStatusChangedAsync(Order order, string templateName)
        {
            if (order == null)
            {
                await _logger.ErrorAsync($"push notification {nameof(EventConsumer)}: order is null.");
                return;
            }
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            if (customer == null)
            {
                await _logger.ErrorAsync($"push notification {nameof(EventConsumer)}: Invalid order customer. customer with id {order.CustomerId} not found.");
                return;
            }

            var phoneNumber =
                await _pushNotificationCustomerService.GetCustomerPhoneNumberAsync(order.CustomerId);

            if (string.IsNullOrEmpty(phoneNumber))
            {
                var orderBillingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                if (orderBillingAddress == null)
                {
                    await _logger.ErrorAsync($"push notification {nameof(EventConsumer)}: Invalid customer phone number and order billing address phone number. order id {order.Id}, billing address id {order.BillingAddressId}");
                    return;
                }

                if (string.IsNullOrEmpty(orderBillingAddress.PhoneNumber))
                {
                    await _logger.ErrorAsync($"push notification {nameof(EventConsumer)}: Invalid customer phone number and order billing address phone number. order id {order.Id}, billing address id {order.BillingAddressId}");
                    return;
                }

                phoneNumber = orderBillingAddress.PhoneNumber;
            }

            await _whatsAppProvider.SendStatusHasChangedAsync(phoneNumber, string.Empty, string.Empty, templateName);
        }

        public async Task HandleEventAsync(OrderPaymentStatusEvent eventMessage)
        {
            var eventName = eventMessage.Name;
            var statusName = eventMessage.Entity.PaymentStatus.ToString();
            var eventConfig = await _eventNotificationConfigService.GetAsync(eventName, statusName);
            
            if(eventConfig == null)
                return;
            
            if(eventConfig.UseFirebase)
                await _eventPushService.HandleEventAsync(eventMessage);
            
            if(eventConfig.UseSms)
                await _handlerService.HandleEventAsync(eventMessage);

            if (eventConfig.UseWhatsApp)
                await SendWhatsAppStatusChangedAsync(eventMessage.Entity, eventConfig.TemplateName);

        }
        
        public async Task HandleEventAsync(OrderShippingStatusEvent eventMessage)
        {
            var eventName = eventMessage.Name;
            var statusName = eventMessage.Entity.ShippingStatus.ToString();
            var eventConfig = await _eventNotificationConfigService.GetAsync(eventName, statusName);
            
            if(eventConfig == null)
                return;
            
            if(eventConfig.UseFirebase)
                await _eventPushService.HandleEventAsync(eventMessage);
            
            if(eventConfig.UseSms)
                await _handlerService.HandleEventAsync(eventMessage);
            
            if (eventConfig.UseWhatsApp)
                await SendWhatsAppStatusChangedAsync(eventMessage.Entity, eventConfig.TemplateName);
        }

        public async Task HandleEventAsync(OrderStatusEvent eventMessage)
        {
            var eventName = eventMessage.Name;
            var statusName = eventMessage.Entity.OrderStatus.ToString();
            var eventConfig = await _eventNotificationConfigService.GetAsync(eventName, statusName);
            
            if(eventConfig == null)
                return;
            
            if(eventConfig.UseFirebase)
                await _eventPushService.HandleEventAsync(eventMessage);
            
            if(eventConfig.UseSms)
                await _handlerService.HandleEventAsync(eventMessage);
            
            if (eventConfig.UseWhatsApp)
                await SendWhatsAppStatusChangedAsync(eventMessage.Entity, eventConfig.TemplateName);
            
        }
    }
}