using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Nop.Plugin.Baramjk.Framework.Services.Jwts
{
    public class ValidationResult
    {
        public ClaimsPrincipal Principal { get; set; }
        public SecurityToken SecurityToken { get; set; }
    }
}