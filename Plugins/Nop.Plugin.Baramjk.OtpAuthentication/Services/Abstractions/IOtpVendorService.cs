using System.Threading.Tasks;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Services.Abstractions
{
    public interface IOtpVendorService
    {
        Task RegisterCustomerAsVendorAsync(Customer customer);
    }
}