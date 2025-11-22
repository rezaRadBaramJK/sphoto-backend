using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Plugin.Baramjk.BackendApi.Services;

namespace Nop.Plugin.Baramjk.BackendApi.Framework.Middleware
{
    public class LanguageMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly BackendLanguageService _backendLanguageService;

        public LanguageMiddleware(
            RequestDelegate next,
            BackendLanguageService backendLanguageService)
        {
            _next = next;
            _backendLanguageService = backendLanguageService;
        }


        public async Task InvokeAsync(HttpContext context, IWorkContext workContext)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Request.Path.StartsWithSegments("/frontendApi", StringComparison.OrdinalIgnoreCase) &&
                context.Request.Headers.TryGetValue(HeaderNames.AcceptLanguage, out var languageCulture) &&
                string.IsNullOrWhiteSpace(languageCulture) == false &&
                languageCulture.Equals("*") == false)
            {
                var culture = languageCulture.ToString().Split(',').First().Trim();

                var currentLanguage = await workContext.GetWorkingLanguageAsync();
                if (currentLanguage.LanguageCulture != culture)
                {
                    var candidLanguage = await _backendLanguageService.GetLanguageByCultureAsync(culture);
                    if (candidLanguage != null)
                        await workContext.SetWorkingLanguageAsync(candidLanguage);
                }
            }


            await _next(context);
        }
    }
}