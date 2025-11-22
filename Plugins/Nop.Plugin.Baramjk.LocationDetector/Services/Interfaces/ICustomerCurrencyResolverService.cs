using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.LocationDetector.Services.Interfaces
{
    public interface ICustomerCurrencyResolverService
    {
        Task SetCustomerCurrencyByCountryAsync(int customerId, int countryId);
    }
}