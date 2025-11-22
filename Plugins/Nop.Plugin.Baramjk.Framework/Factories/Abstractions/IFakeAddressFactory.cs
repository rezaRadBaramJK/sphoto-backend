using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Baramjk.Framework.Factories.Abstractions
{
    public interface IFakeAddressFactory
    {
        Task<Address> CreateAsync(
            Customer customer = null,
            string firstName = "",
            string lastName = "",
            string email = "",
            string company = "Baramjk",
            int countryId = 0,
            string city = "dummy city", 
            string address1 = "dummy address1",
            string address2 = "dummy address2",
            string zipPostalCode = "1234",
            string phoneNumber = "",
            string faxNumber = "2");
    }
}