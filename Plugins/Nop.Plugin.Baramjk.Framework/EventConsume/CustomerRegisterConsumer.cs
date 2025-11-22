using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Plugin.Baramjk.Framework.Events.Customers;
using Nop.Services.Customers;
using Nop.Services.Events;

namespace Nop.Plugin.Baramjk.Framework.EventConsume
{
    public class CustomerRegisterConsumer : IConsumer<EntityInsertedEvent<CustomerCustomerRoleMapping>>
    {
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;

        public CustomerRegisterConsumer(ICustomerService customerService, IEventPublisher eventPublisher)
        {
            _customerService = customerService;
            _eventPublisher = eventPublisher;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<CustomerCustomerRoleMapping> eventMessage)
        {
            var registeredRole =
                await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
            var isRegisteredRole = eventMessage.Entity.CustomerRoleId == registeredRole.Id;
            if (isRegisteredRole == false)
                return;

            var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Entity.CustomerId);
            var baramjkEvent = new CustomerRegisterEvent(customer);
            await _eventPublisher.PublishAsync(baramjkEvent);
        }
    }
}