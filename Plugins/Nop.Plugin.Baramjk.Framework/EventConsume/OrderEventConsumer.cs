using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Plugin.Baramjk.Framework.Events;
using Nop.Plugin.Baramjk.Framework.Events.Orders;
using Nop.Services.Common;
using Nop.Services.Events;

namespace Nop.Plugin.Baramjk.Framework.EventConsume
{
    public class OrderEventConsumer : IConsumer<EntityUpdatedEvent<Order>>
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;

        public OrderEventConsumer(IEventPublisher eventPublisher, IGenericAttributeService genericAttributeService)
        {
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Order> eventMessage)
        {
            await SendEventAsync(eventMessage);

            await _genericAttributeService.SaveAttributeAsync(eventMessage.Entity,
                EventKeys.OrderPaymentStatusKey, eventMessage.Entity.PaymentStatus);

            await _genericAttributeService.SaveAttributeAsync(eventMessage.Entity,
                EventKeys.OrderShippingStatusKey, eventMessage.Entity.ShippingStatus);

            await _genericAttributeService.SaveAttributeAsync(eventMessage.Entity,
                EventKeys.OrderOrderStatusKey, eventMessage.Entity.OrderStatus);
        }

        private async Task SendEventAsync(EntityUpdatedEvent<Order> eventMessage)
        {
            var order = eventMessage.Entity;
            var attributesForEntity = await _genericAttributeService.GetAttributesForEntityAsync(order.Id, "Order");

            var paymentStatusValue = attributesForEntity.FirstOrDefault(item =>
                item.Key == EventKeys.OrderPaymentStatusKey)?.Value;

            var shippingStatusValue = attributesForEntity.FirstOrDefault(item =>
                item.Key == EventKeys.OrderShippingStatusKey)?.Value;

            var orderStatusValue = attributesForEntity.FirstOrDefault(item =>
                item.Key == EventKeys.OrderOrderStatusKey)?.Value;

            if (paymentStatusValue != order.PaymentStatus.ToString())
                await _eventPublisher.PublishAsync(new OrderPaymentStatusEvent(order));

            if (shippingStatusValue != order.ShippingStatus.ToString())
                await _eventPublisher.PublishAsync(new OrderShippingStatusEvent(order));

            if (orderStatusValue != order.OrderStatus.ToString())
                await _eventPublisher.PublishAsync(new OrderStatusEvent(order));
        }
    }
}