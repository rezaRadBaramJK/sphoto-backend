using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Baramjk.Framework.Events.Orders
{
    public class OrderEvent : BaramjkEvent<Order>
    {
        public OrderEvent(Order entity, string name) : base(entity, name)
        {
        }
    }
}