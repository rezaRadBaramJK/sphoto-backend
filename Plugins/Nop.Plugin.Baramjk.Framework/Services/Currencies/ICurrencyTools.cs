using System.Threading.Tasks;
using Nop.Core.Domain.Directory;

namespace Nop.Plugin.Baramjk.Framework.Services.Currencies
{
    public interface ICurrencyTools
    {
        Task<Currency> GetPrimaryCurrencyAsync();
        Task<CurrencyModel> ConvertPrimaryToCustomerCurrencyAsync(decimal amount);
        Task<CurrencyModel> ConvertPrimaryToAsync(decimal amount, string code);
        Task<CurrencyModel> ConvertCurrencyAsync(CurrencyConvertRequest request);
        CurrencyModel CreateCurrencyModel(decimal amount, Currency currency);
        string GetCurrencyString(decimal amount, bool showCurrency, Currency targetCurrency);
        Task<CurrencyModel> ConvertToPrimaryAsync(decimal amount, string fromCode);
    }
}