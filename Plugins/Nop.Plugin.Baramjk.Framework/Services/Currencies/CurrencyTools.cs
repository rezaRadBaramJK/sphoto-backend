using System;
using System.Globalization;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Directory;

namespace Nop.Plugin.Baramjk.Framework.Services.Currencies
{
    public class CurrencyTools : ICurrencyTools
    {
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;

        public CurrencyTools(ICurrencyService currencyService, IWorkContext workContext,
            CurrencySettings currencySettings, IPriceFormatter priceFormatter)
        {
            _currencyService = currencyService;
            _workContext = workContext;
            _currencySettings = currencySettings;
            _priceFormatter = priceFormatter;
        }

        public async Task<Currency> GetPrimaryCurrencyAsync()
        {
            var primaryCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            return primaryCurrency;
        }

        public async Task<CurrencyModel> ConvertPrimaryToCustomerCurrencyAsync(decimal amount)
        {
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var primaryCurrency = await GetPrimaryCurrencyAsync();
            var result = await _currencyService.ConvertCurrencyAsync(amount, primaryCurrency, currentCurrency);
            var currencyModel = CreateCurrencyModel(result, currentCurrency);
            return currencyModel;
        }

        public async Task<CurrencyModel> ConvertPrimaryToAsync(decimal amount, string code)
        {
            var toCurrency = await _currencyService.GetCurrencyByCodeAsync(code);
            var primaryCurrency = await GetPrimaryCurrencyAsync();
            var result = await _currencyService.ConvertCurrencyAsync(amount, primaryCurrency, toCurrency);
            var currencyModel = CreateCurrencyModel(result, toCurrency);
            return currencyModel;
        }

        public async Task<CurrencyModel> ConvertToPrimaryAsync(decimal amount, string fromCode)
        {
            var fromCurrency = await _currencyService.GetCurrencyByCodeAsync(fromCode);
            var primaryCurrency = await GetPrimaryCurrencyAsync();
            var result = await _currencyService.ConvertCurrencyAsync(amount, fromCurrency, primaryCurrency);
            var currencyModel = CreateCurrencyModel(result, primaryCurrency);
            return currencyModel;
        }

        public async Task<CurrencyModel> ConvertCurrencyAsync(CurrencyConvertRequest request)
        {
            var currencyFrom = await _currencyService.GetCurrencyByCodeAsync(request.FromCode);
            var currencyTo = await _currencyService.GetCurrencyByCodeAsync(request.ToCode);
            var result = await _currencyService.ConvertCurrencyAsync(request.Amount, currencyFrom, currencyTo);
            var currencyModel = CreateCurrencyModel(result, currencyTo);
            return currencyModel;
        }

        public CurrencyModel CreateCurrencyModel(decimal amount, Currency currency)
        {
            var currencyModel = new CurrencyModel
            {
                Amount = amount,
                Display = GetCurrencyString(amount, true, currency)
            };
            return currencyModel;
        }

        public virtual string GetCurrencyString(decimal amount, bool showCurrency, Currency targetCurrency)
        {
            if (targetCurrency == null)
                throw new ArgumentNullException(nameof(targetCurrency));

            string result;
            if (!string.IsNullOrEmpty(targetCurrency.CustomFormatting))
                //custom formatting specified by a store owner
            {
                result = amount.ToString(targetCurrency.CustomFormatting);
            }
            else
            {
                if (!string.IsNullOrEmpty(targetCurrency.DisplayLocale))
                    //default behavior
                {
                    result = amount.ToString("C", new CultureInfo(targetCurrency.DisplayLocale));
                }
                else
                {
                    //not possible because "DisplayLocale" should be always specified
                    //but anyway let's just handle this behavior
                    result = $"{amount:N} ({targetCurrency.CurrencyCode})";
                    return result;
                }
            }

            //display currency code?
            if (showCurrency && _currencySettings.DisplayCurrencyLabel)
                result = $"{result} ({targetCurrency.CurrencyCode})";
            return result;
        }
    }
}