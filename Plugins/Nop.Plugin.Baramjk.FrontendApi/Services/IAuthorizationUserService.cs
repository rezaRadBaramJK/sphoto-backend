using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Models;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public interface IAuthorizationUserService
    {
        Task<AuthenticateResponse> AuthenticateAsync(AuthenticateCustomerRequest request);
        Task<AuthenticateResponse> AuthenticateAsync(Customer customer);
    }
}