using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Models.Customers;

namespace Nop.Plugin.Baramjk.Framework.Factories
{
    public interface ICustomerDtoFactory
    {
        Task<CustomerDto> PrepareCustomerDtoAsync(int customerId);
        Task<CustomerDto> PrepareCustomerDtoAsync(Customer customer);
        Task<CustomerDto> PrepareCustomerDtoAsync(int customerId, int addressId);
        Task<CustomerDto> PrepareCustomerDtoAsync(Customer customer, int addressId);
    }
}