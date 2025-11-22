using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Baramjk.BackendApi.Framework.Services
{
    public interface IJwtTokenService
    {
        /// <summary>
        ///     Create a new secret key
        /// </summary>
        string NewSecretKey { get; }

        /// <summary>
        ///     Generate new JWT token
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <returns>JWT token</returns>
        string GetNewJwtToken(Customer customer);
    }
}