using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.LocationDetector.Services.Interfaces;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;

namespace Nop.Plugin.Baramjk.LocationDetector.Services
{
    public class CustomerCurrencyResolverService : ICustomerCurrencyResolverService
    {
        private readonly LocationDetectorSettings _settings;
        private readonly ICountryService _countryService;
        private readonly ILocationCurrencyService _locationCurrencyService;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        public CustomerCurrencyResolverService(LocationDetectorSettings settings,
            ICountryService countryService,
            ILocationCurrencyService locationCurrencyService,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            IWorkContext workContext)
        {
            _settings = settings;
            _countryService = countryService;
            _locationCurrencyService = locationCurrencyService;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
            _workContext = workContext;
        }

        public async Task SetCustomerCurrencyByCountryAsync(int customerId, int countryId)
        {
            // events raise on changing country,
            // but we only need to change the working currency of user
            // whenever the SwitchCustomerCurrencyOnChangingCountry settings is turned on
            
            if (_settings.SwitchCustomerCurrencyOnChangingCountry)
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerId);

                var country = await _countryService.GetCountryByIdAsync(countryId);

                var currencyCode = await _locationCurrencyService.GetCurrencyByLocation(country.ThreeLetterIsoCode);

                var currency = await _currencyService.GetCurrencyByCodeAsync(currencyCode);
                if (currency != null)
                {
                    await _workContext.SetWorkingCurrencyAsync(currency);
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CurrencyIdAttribute, currency.Id, 0);
                }
            }
        }
    }
}