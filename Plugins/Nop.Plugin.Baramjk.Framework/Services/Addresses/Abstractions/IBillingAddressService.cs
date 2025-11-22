using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Baramjk.Framework.Services.Addresses.Abstractions
{
    public interface IBillingAddressService
    {
        Task<Address> HandelAsync(Customer customer);
    }
}