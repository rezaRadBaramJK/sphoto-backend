using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Factories.Abstractions;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;

namespace Nop.Plugin.Baramjk.Framework.Factories
{
    public class FakeAddressFactory : IFakeAddressFactory
    {
        private readonly IWorkContext _workContext;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAddressService _addressService;

        public FakeAddressFactory(
            IWorkContext workContext,
            ICountryService countryService, 
            ILocalizationService localizationService,
            IGenericAttributeService genericAttributeService,
            IAddressService addressService)
        {
            _workContext = workContext;
            _countryService = countryService;
            _localizationService = localizationService;
            _genericAttributeService = genericAttributeService;
            _addressService = addressService;
        }

        public async Task<Address> CreateAsync(
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
            string faxNumber = "2")
        {
            customer ??= await _workContext.GetCurrentCustomerAsync();

            if (string.IsNullOrEmpty(email))
                email = $"{customer.Id}@baramjk.com";
            else if (CommonHelper.IsValidEmail(email) == false)
                throw new ArgumentException($"Invalid {email}");

            firstName = await GetAttributeAsync(firstName, NopCustomerDefaults.FirstNameAttribute, "f", customer);
            lastName = await GetAttributeAsync(lastName, NopCustomerDefaults.LastNameAttribute, "l", customer);
            phoneNumber = await GetAttributeAsync(phoneNumber, NopCustomerDefaults.PhoneAttribute, "1", customer);
            
            Country country;
            
            if (countryId <= 0)
            {
                country = await _countryService.GetCountryByThreeLetterIsoCodeAsync("KWT");
                if (country == null)
                    throw new InternalErrorBusinessException("Kwt country not found.");
                countryId = country.Id;
            }
            else
                country = await _countryService.GetCountryByIdAsync(countryId);

            var countryName = await _localizationService.GetLocalizedAsync(country, c => c.Name);
            var address = new Address
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Company = company,
                CountryId = countryId,
                County = countryName,
                City = city,
                Address1 = address1,
                Address2 = address2,
                ZipPostalCode = zipPostalCode,
                PhoneNumber = phoneNumber,
                FaxNumber = faxNumber,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _addressService.InsertAddressAsync(address);
            return address;
        }


        private async Task<string> GetAttributeAsync(
            string value,
            string attributeName,
            string defaultValue,
            Customer customer)
        {
            if (string.IsNullOrEmpty(value) == false)
                return value;
            
            return await _genericAttributeService.GetAttributeAsync(customer, attributeName, defaultValue: defaultValue);
        }
    }
}