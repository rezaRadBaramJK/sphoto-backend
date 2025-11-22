using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Baramjk.Framework.Events.Orders
{
    public class OrderStatusBaseEvent : OrderEvent
    {
        public OrderStatusBaseEvent(Order entity, string name) : base(entity, name)
        {
        }
    }

    public class OrderPaymentStatusEvent : OrderStatusBaseEvent
    {
        public OrderPaymentStatusEvent(Order entity) : base(entity, EventKeys.OrderPaymentStatusKey)
        {
        }
    }

    public class OrderShippingStatusEvent : OrderStatusBaseEvent
    {
        public OrderShippingStatusEvent(Order entity) : base(entity, EventKeys.OrderShippingStatusKey)
        {
        }
    }

    public class OrderStatusEvent : OrderStatusBaseEvent
    {
        public OrderStatusEvent(Order entity) : base(entity, EventKeys.OrderOrderStatusKey)
        {
        }
    }
}