using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Api;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Services.Abstractions
{
    public interface IAuthorizationUserService
    {
        Task<AuthenticateResponse> AuthenticateAsync(AuthenticateCustomerRequest request);
        Task<AuthenticateResponse> AuthenticateAsync(Customer customer);
    }
}