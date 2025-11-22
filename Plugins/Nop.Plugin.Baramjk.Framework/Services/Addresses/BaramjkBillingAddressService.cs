using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Services.Addresses.Abstractions;
using Nop.Services.Common;
using Nop.Services.Customers;

namespace Nop.Plugin.Baramjk.Framework.Services.Addresses
{
    public class BaramjkBillingAddressService: IBillingAddressService
    {
        private readonly IAddressService _addressService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;

        public BaramjkBillingAddressService(
            IAddressService addressService,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService)
        {
            _addressService = addressService;
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
        }

        public async Task<Address> HandelAsync(Customer customer)
        {
            var addresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);
            Address billingAddress;
            if (addresses.Count == 0)
            {
                billingAddress = await CreateBillingAddressAsync(customer);
                await _customerService.InsertCustomerAddressAsync(customer, billingAddress);
            }
            else
                billingAddress = addresses.FirstOrDefault();
            
            customer.BillingAddressId = billingAddress?.Id;
            await _customerService.UpdateCustomerAsync(customer);
            return billingAddress;
        }

        private async Task<Address> CreateBillingAddressAsync(Customer customer)
        {
            var customerFirstName = await _genericAttributeService.GetAttributeAsync(customer, NopCustomerDefaults.FirstNameAttribute, defaultValue: string.Empty);
            var customerLastName = await _genericAttributeService.GetAttributeAsync(customer, NopCustomerDefaults.LastNameAttribute, defaultValue: string.Empty);
            var customerPhoneNumber = await _genericAttributeService.GetAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute, defaultValue: string.Empty);
            
            var billingAddress = new Address
            {
                Email = customer.Email,
                FirstName = customerFirstName,
                LastName = customerLastName,
                PhoneNumber = customerPhoneNumber,
                CreatedOnUtc = DateTime.UtcNow,
            };

            await _addressService.InsertAddressAsync(billingAddress);
            return billingAddress;
        }
    }
}