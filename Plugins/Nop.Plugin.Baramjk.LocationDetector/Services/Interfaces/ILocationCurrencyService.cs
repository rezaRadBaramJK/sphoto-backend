using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.LocationDetector.Services.Interfaces
{
    public interface ILocationCurrencyService
    {
        Task<string> GetCurrencyByLocation(string isoCountry);
        Task<int> GetCurrencyIdByCurrencyCode(string code);
        

    }
}