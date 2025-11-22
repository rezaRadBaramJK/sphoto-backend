using System;
using System.Linq;
using Nop.Plugin.Baramjk.Framework.Services.Jwts;
using Nop.Plugin.Baramjk.Framework.Services.License.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.License
{
    public class LicenseParser : ILicenseParser
    {
        private readonly IJwtService _jwtService;

        public LicenseParser(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public PluginLicense Pars(string license)
        {
            var result = _jwtService.ValidateToken(license, LicenseDefault.ScrteKey, LicenseDefault.Issuer,
                LicenseDefault.Audience);

            var plugins = result.Principal.Claims.Where(item => item.Type == LicenseDefault.PluginsClaimType)
                .Select(item => item.Value)
                .ToList();

            var domains = result.Principal.Claims.Where(item => item.Type == LicenseDefault.DomainsClaimType)
                .Select(item => item.Value)
                .ToList();

            var type = result.Principal.Claims.Where(item => item.Type == LicenseDefault.TypeClaimType)
                .Select(item => item.Value)
                .FirstOrDefault();

            var pluginLicense = new PluginLicense
            {
                Domains = domains,
                Plugins = plugins,
                License = license,
                Type = type,
                ExpireDateTime = result.SecurityToken.ValidTo
            };

            return pluginLicense;
        }

        public bool TryPars(string license, out PluginLicense pluginLicense)
        {
            try
            {
                var result = _jwtService.ValidateToken(license, LicenseDefault.ScrteKey, LicenseDefault.Issuer,
                    LicenseDefault.Audience);

                var plugins = result.Principal.Claims.Where(item => item.Type == LicenseDefault.PluginsClaimType)
                    .Select(item => item.Value)
                    .ToList();

                var domains = result.Principal.Claims.Where(item => item.Type == LicenseDefault.DomainsClaimType)
                    .Select(item => item.Value)
                    .ToList();

                var type = result.Principal.Claims.Where(item => item.Type == LicenseDefault.TypeClaimType)
                    .Select(item => item.Value)
                    .FirstOrDefault();

                pluginLicense = new PluginLicense
                {
                    Domains = domains,
                    Plugins = plugins,
                    License = license,
                    Type = type,
                    ExpireDateTime = result.SecurityToken.ValidTo
                };

                return true;
            }
            catch (Exception)
            {
                pluginLicense = null;
                return false;
            }
        }
    }
}