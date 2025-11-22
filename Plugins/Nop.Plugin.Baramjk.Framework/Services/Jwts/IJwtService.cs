using System;
using System.Security.Claims;

namespace Nop.Plugin.Baramjk.Framework.Services.Jwts
{
    public interface IJwtService
    {
        string GenerateToken(string key, string issuer = null, string audience = null, DateTime? expire = null,
            params Claim[] claims);

        ValidationResult ValidateToken(string token, string key, string issuer = null, string audience = null,
            bool validateLifetime = true);
    }
}