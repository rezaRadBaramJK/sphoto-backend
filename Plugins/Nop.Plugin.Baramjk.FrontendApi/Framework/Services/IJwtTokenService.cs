using System;
using System.Collections.Generic;
using System.Security.Claims;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Baramjk.FrontendApi.Framework.Services
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

        string GetJwtToken(List<Claim> claims, DateTime? expireTime = null);
    }
}