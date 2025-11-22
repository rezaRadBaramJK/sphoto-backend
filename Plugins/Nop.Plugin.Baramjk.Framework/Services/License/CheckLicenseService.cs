using System;
using Microsoft.AspNetCore.Http;

namespace Nop.Plugin.Baramjk.Framework.Services.License
{
    public class CheckLicenseService : ILicenseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILicenseManagerService _licenseManagerService;

        public CheckLicenseService(ILicenseManagerService licenseManagerService,
            IHttpContextAccessor httpContextAccessor)
        {
            _licenseManagerService = licenseManagerService;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsLicensed(string pluginName)
        {
            var license = _licenseManagerService.GetPluginLicense(pluginName);
            if (license == null)
                return false;

            if (license.ExpireDateTime <= DateTime.Now)
                return false;

            if (license.Plugins.Contains(pluginName) == false)
                return false;

            var host = _httpContextAccessor.HttpContext.Request.Host.Host.ToLower();
            if (host.StartsWith("www."))
                host = host.Replace("www.", "");

            foreach (var domain in license.Domains)
            {
                if (host == domain.ToLower())
                    return true;

                if (domain.StartsWith("*") == false)
                    continue;

                if (host.EndsWith(domain.Replace("*", "")))
                    return true;
            }

            return false;
        }
    }
}