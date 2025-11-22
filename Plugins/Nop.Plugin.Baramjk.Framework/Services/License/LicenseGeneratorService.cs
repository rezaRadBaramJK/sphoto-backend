using System;
using System.Security.Claims;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Services.Jwts;

namespace Nop.Plugin.Baramjk.Framework.Services.License
{
    public class LicenseGeneratorService
    {
        private readonly IJwtService _jwtService;
        private readonly ILicenseParser _licenseParser;
        private readonly IRepository<GenericAttribute> _repositoryGenericAttribute;

        public LicenseGeneratorService(ILicenseParser licenseParser,
            IRepository<GenericAttribute> repositoryGenericAttribute, IJwtService jwtService)
        {
            _licenseParser = licenseParser;
            _repositoryGenericAttribute = repositoryGenericAttribute;
            _jwtService = jwtService;
        }

        public string CreatePluginLicense(string pluginName)
        {
            var domainsClaim = new Claim(LicenseDefault.DomainsClaimType, "Localhost");
            var typeClaim = new Claim(LicenseDefault.TypeClaimType, "Normal");
            var pluginsClaim = new Claim(LicenseDefault.PluginsClaimType, "Baramjk.Auction");

            var token = _jwtService.GenerateToken(LicenseDefault.ScrteKey,
                audience: LicenseDefault.Audience,
                issuer: LicenseDefault.Issuer,
                expire: DateTime.Now.AddDays(10), claims: new[]
                {
                    domainsClaim,
                    typeClaim,
                    pluginsClaim
                });

            return token;
        }
    }
}