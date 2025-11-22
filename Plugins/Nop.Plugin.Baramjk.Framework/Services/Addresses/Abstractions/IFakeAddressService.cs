using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Services.Addresses.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.Addresses.Abstractions
{
    public interface IFakeAddressService
    {
        Task<Address> SetBillingAddressAsync(Customer customer, IList<Address> addresses = null);

        Task<Address> SetShippingAddressAsync(Customer customer, IList<Address> addresses = null);

        Task<SetAddressServiceResults> SetBillingAndShippingAddressAsync(Customer customer);
    }
}