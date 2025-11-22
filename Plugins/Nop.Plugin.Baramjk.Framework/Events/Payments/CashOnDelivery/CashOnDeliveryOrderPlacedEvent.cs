using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Baramjk.Framework.Events.Payments.CashOnDelivery
{
    public class CashOnDeliveryOrderPlacedEvent: BaramjkEvent<Order>
    {
        public CashOnDeliveryOrderPlacedEvent(Order entity) : base(entity)
        {
        }

        public CashOnDeliveryOrderPlacedEvent(Order entity, string name) : base(entity, name)
        {
        }
        
    }
}