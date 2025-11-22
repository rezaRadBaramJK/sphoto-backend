using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Nop.Plugin.Baramjk.Framework.Services.Jwts
{
    public class JwtService : IJwtService
    {
        public string GenerateToken(string key, string issuer = null, string audience = null, DateTime? expire = null,
            params Claim[] claims)
        {
            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(issuer, audience, claims, expires: expire,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        public ValidationResult ValidateToken(string token, string key, string issuer = null, string audience = null,
            bool validateLifetime = true)
        {
            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = issuer,
                ValidAudience = audience,
                ValidateLifetime = true,
                IssuerSigningKey = jwtKey
            };

            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, validationParameters, out var securityToken);

            var validationResult = new ValidationResult
            {
                Principal = principal,
                SecurityToken = securityToken
            };

            return validationResult;
        }
    }
}