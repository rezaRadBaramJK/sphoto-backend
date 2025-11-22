using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.OtpAuthentication.Consts;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Settings;
using Nop.Plugin.Baramjk.OtpAuthentication.Services.Abstractions;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        #region Ctor

        public JwtTokenService(CustomerSettings customerSettings,
            FrontendApiSettings webApiCommonSettings)
        {
            _customerSettings = customerSettings;
            _webApiCommonSettings = webApiCommonSettings;
        }

        #endregion

        /// <summary>
        ///     Generate new JWT token
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <returns>JWT token</returns>
        public virtual string GetNewJwtToken(Customer customer, int expirationSeconds)
        {
            // generate token that is valid for 7 days (by default)
            var currentTime = DateTimeOffset.Now;
            var expiresInSeconds = currentTime.AddSeconds(expirationSeconds).ToUnixTimeSeconds();

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Nbf, currentTime.ToUnixTimeSeconds().ToString()),
                new(JwtRegisteredClaimNames.Exp, expiresInSeconds.ToString()),
                new(WebApiCommonDefaults.ClaimTypeName, customer.Id.ToString()),
                new(ClaimTypes.NameIdentifier, customer.CustomerGuid.ToString())
            };

            if (_customerSettings.UsernamesEnabled)
            {
                if (!string.IsNullOrEmpty(customer.Username))
                    claims.Add(new Claim(ClaimTypes.Name, customer.Username));
            }
            else
            {
                if (!string.IsNullOrEmpty(customer.Email))
                    claims.Add(new Claim(ClaimTypes.Email, customer.Email));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_webApiCommonSettings.SecretKey);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                WebApiCommonDefaults.JwtSignatureAlgorithm);
            var token = new JwtSecurityToken(new JwtHeader(signingCredentials), new JwtPayload(claims));

            return tokenHandler.WriteToken(token);
        }

        public virtual string GetJwtToken(List<Claim> claims, DateTime? expireTime = null)
        {
            if (expireTime != null)
            {
                var expiresInSeconds = new DateTimeOffset(expireTime.Value).ToUnixTimeSeconds();

                claims.Add(new Claim(JwtRegisteredClaimNames.Exp, expiresInSeconds.ToString()));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_webApiCommonSettings.SecretKey);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                WebApiCommonDefaults.JwtSignatureAlgorithm);
            var token = new JwtSecurityToken(new JwtHeader(signingCredentials), new JwtPayload(claims));

            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        ///     Create a new secret key
        /// </summary>
        public virtual string NewSecretKey
        {
            get
            {
                //generate a cryptographic random number
                using var provider = new RNGCryptoServiceProvider();
                var buff = new byte[WebApiCommonDefaults.MinSecretKeyLength];
                provider.GetBytes(buff);

                // Return a Base64 string representation of the random number
                return Convert.ToBase64String(buff).TrimEnd('=');
            }
        }

        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly FrontendApiSettings _webApiCommonSettings;

        #endregion
    }
}