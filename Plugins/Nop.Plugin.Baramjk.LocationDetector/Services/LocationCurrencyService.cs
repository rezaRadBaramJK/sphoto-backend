using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.LocationDetector.Domain;
using Nop.Plugin.Baramjk.LocationDetector.Services.Interfaces;
using Nop.Services.Directory;

namespace Nop.Plugin.Baramjk.LocationDetector.Services
{
    public class LocationCurrencyService : ILocationCurrencyService
    {
        private readonly IRepository<CountryCurrencyMapping> _repository;
        private readonly LocationDetectorSettings _settings;
        private readonly ICurrencyService _currencyService;

        public LocationCurrencyService(IRepository<CountryCurrencyMapping> repository,
            LocationDetectorSettings settings,
            ICurrencyService currencyService)
        {
            _repository = repository;
            _settings = settings;
            _currencyService = currencyService;

        }

        public async Task<string> GetCurrencyByLocation(string isoCountry)
        {
            var mapping = await _repository.Table.Where(x => x.IsoCountryCode == isoCountry).FirstOrDefaultAsync();
            if (mapping != default)
            {
                return mapping.IsoCurrencyCode;
            }

            return _settings.DefaultCurrency;
        }

        public async Task<int> GetCurrencyIdByCurrencyCode(string code)
        {
            var currency = await _currencyService.GetCurrencyByCodeAsync(code);
            if (currency == default)
            {
                throw new Exception($"Currency {code} not found");
            }

            return currency.Id;
        }
    }
}