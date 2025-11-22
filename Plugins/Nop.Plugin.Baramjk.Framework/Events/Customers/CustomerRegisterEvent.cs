using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Baramjk.Framework.Events.Customers
{
    public class CustomerRegisterEvent : CustomerEvent
    {
        public CustomerRegisterEvent(Customer entity) : base(entity, EventKeys.CustomerRegister)
        {
        }
    }
}