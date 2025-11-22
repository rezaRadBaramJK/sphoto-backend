using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Currencies;
using Nop.Services.Localization;

namespace Nop.Plugin.Baramjk.FrontendApi.Factories
{
    public class CurrencyFactory
    {
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;

        public CurrencyFactory(
            IWorkContext workContext,
            ILocalizationService localizationService)
        {
            _workContext = workContext;
            _localizationService = localizationService;
        }

        public async Task<List<CurrencyDto>> PrepareCurrencyDtoListAsync(IList<Currency> currencies)
        {
            var selectedCurrency = await _workContext.GetWorkingCurrencyAsync();
            var selectedLanguage = await _workContext.GetWorkingLanguageAsync();
            return await currencies
                .SelectAwait(async currency => await PrepareCurrencyAsync(currency, selectedCurrency, selectedLanguage))
                .ToListAsync();
        }

        public async Task<CurrencyDto> PrepareCurrencyAsync(
            Currency currency,
            Currency selectedCurrency = null,
            Language selectedLanguage = null)
        {
            selectedCurrency ??= await _workContext.GetWorkingCurrencyAsync();
            selectedLanguage ??= await _workContext.GetWorkingLanguageAsync();
            
            var dto = AutoMapperConfiguration.Mapper.Map<CurrencyDto>(currency);
            dto.IsSelected = currency.Id == selectedCurrency.Id;
            dto.Name = await _localizationService.GetLocalizedAsync(currency, c => c.Name);
            dto.LocalizationCurrencyCode = string.IsNullOrEmpty(currency.DisplayLocale) || 
                                           selectedLanguage.UniqueSeoCode.Equals("en", StringComparison.OrdinalIgnoreCase)
                ? currency.CurrencyCode
                : new RegionInfo(currency.DisplayLocale).CurrencySymbol;
            return dto;
        }
        
    }
}