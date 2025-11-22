using Microsoft.AspNetCore.Http;

namespace Nop.Plugin.Baramjk.Framework.Extensions
{
    public static class HttpContextAccessorEx
    {
        public static bool HasQueryTrue(this IHttpContextAccessor accessor, string key)
        {
            bool.TryParse(accessor?.HttpContext?.Request.Query[key], out var result);
            return result;
        }
    }
}