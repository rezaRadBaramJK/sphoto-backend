using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Directory;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Country;
using Nop.Services.Localization;

namespace Nop.Plugin.Baramjk.FrontendApi.Factories
{
    public class CountryFactory
    {
        private readonly ILocalizationService _localizationService;

        public CountryFactory(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public async Task<IList<CountryDto>> PrepareCountriesAsync(IList<Country> countries)
        {
            return await countries.SelectAwait(async country =>
            {
                var countryDto = AutoMapperConfiguration.Mapper.Map<CountryDto>(country);
                countryDto.Name = await _localizationService.GetLocalizedAsync(country, c => c.Name);
                return countryDto;
            }).ToListAsync();
        }
    }
}