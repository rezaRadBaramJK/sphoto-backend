using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Tax;
using Nop.Core.Security;
using Nop.Plugin.Baramjk.LocationDetector.Services.Interfaces;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.LocationDetector.Services
{
    public class SetCurrencyWebWorkContext : WebWorkContext
    {
        private readonly ILocationDetector _locationDetector;
        private readonly ILocationCurrencyService _locationCurrencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;

        public SetCurrencyWebWorkContext(CookieSettings cookieSettings, CurrencySettings currencySettings,
            IAuthenticationService authenticationService, ICurrencyService currencyService, ICustomerService customerService,
            IGenericAttributeService genericAttributeService, IHttpContextAccessor httpContextAccessor, ILanguageService languageService,
            IStoreContext storeContext, IStoreMappingService storeMappingService, IUserAgentHelper userAgentHelper, IVendorService vendorService,
            IWebHelper webHelper, LocalizationSettings localizationSettings, TaxSettings taxSettings, ILocationDetector locationDetector,
            ILocationCurrencyService locationCurrencyService, ILogger logger) : base(cookieSettings, currencySettings, authenticationService,
            currencyService, customerService, genericAttributeService, httpContextAccessor, languageService, storeContext, storeMappingService,
            userAgentHelper, vendorService, webHelper, localizationSettings, taxSettings)
        {
            _genericAttributeService = genericAttributeService;
            _webHelper = webHelper;
            _locationDetector = locationDetector;
            _locationCurrencyService = locationCurrencyService;
            _logger = logger;
        }

        public override async Task<Currency> GetWorkingCurrencyAsync()
        {
            var customer = await GetCurrentCustomerAsync();
            var customerCurrencyId = await _genericAttributeService
                .GetAttributeAsync<int>(customer, NopCustomerDefaults.CurrencyIdAttribute, 0);
            if (customerCurrencyId == default || customerCurrencyId == 0)
            {
                var ip = _webHelper.GetCurrentIpAddress();
                if (ip != "127.0.0.1")
                {
                    var isoCountry = await _locationDetector.GetLocationByIp(ip);
                    var currencyCode = await _locationCurrencyService.GetCurrencyByLocation(isoCountry);
                    var currencyId = await _locationCurrencyService.GetCurrencyIdByCurrencyCode(currencyCode);
                    await _logger.InformationAsync($"GetCurrentCustomerAsync ip:{ip},{isoCountry},currency:{currencyCode}");

                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CurrencyIdAttribute, currencyId, 0);
                }
            }

            return await base.GetWorkingCurrencyAsync();
        }
    }
}