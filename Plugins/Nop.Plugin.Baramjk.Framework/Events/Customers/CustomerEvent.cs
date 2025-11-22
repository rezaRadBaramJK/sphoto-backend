using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Baramjk.Framework.Events.Customers
{
    public class CustomerEvent : BaramjkEvent<Customer>
    {
        public CustomerEvent(Customer entity, string name) : base(entity, name)
        {
        }
    }
}