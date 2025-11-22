using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Factories.Abstractions;
using Nop.Plugin.Baramjk.Framework.Services.Addresses.Abstractions;
using Nop.Plugin.Baramjk.Framework.Services.Addresses.Models;
using Nop.Services.Customers;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class FrontendApiFakeAddressService : IFakeAddressService
    {
        private readonly IFakeAddressFactory _fakeAddressFactory;
        private readonly ICustomerService _customerService;

        public FrontendApiFakeAddressService(
            IFakeAddressFactory fakeAddressFactory,
            ICustomerService customerService)
        {
            _fakeAddressFactory = fakeAddressFactory;
            _customerService = customerService;
        }

        public async Task<Address> SetBillingAddressAsync(Customer customer, IList<Address> addresses = null)
        {
            addresses ??= await _customerService.GetAddressesByCustomerIdAsync(customer.Id);

            Address billingAddress;
            if (addresses.Count == 0)
            {
                billingAddress = await _fakeAddressFactory.CreateAsync(customer);
                await _customerService.InsertCustomerAddressAsync(customer, billingAddress);
            }
            else
                billingAddress = addresses.FirstOrDefault();

            customer.BillingAddressId = billingAddress?.Id;
            await _customerService.UpdateCustomerAsync(customer);
            return billingAddress;
        }

        public async Task<Address> SetShippingAddressAsync(Customer customer, IList<Address> addresses = null)
        {
            addresses ??= await _customerService.GetAddressesByCustomerIdAsync(customer.Id);
            Address shippingAddress;
            if (addresses.Count == 0)
            {
                shippingAddress = await _fakeAddressFactory.CreateAsync(customer);
                await _customerService.InsertCustomerAddressAsync(customer, shippingAddress);
            }
            else
                shippingAddress = addresses.FirstOrDefault();

            customer.ShippingAddressId = shippingAddress?.Id;
            await _customerService.UpdateCustomerAsync(customer);
            return shippingAddress;
        }

        public async Task<SetAddressServiceResults> SetBillingAndShippingAddressAsync(Customer customer)
        {
            var addresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);
            var billingAddress = await SetBillingAddressAsync(customer, addresses);
            var shippingAddress = await SetShippingAddressAsync(customer, addresses);
            
            return new SetAddressServiceResults
            {
                BillingAddress = billingAddress,
                ShippingAddress = shippingAddress
            };
        }
    }
}