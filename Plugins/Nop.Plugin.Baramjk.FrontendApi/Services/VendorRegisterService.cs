using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.FrontendApi.Services.Models.CustomerVendor;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Vendors;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class VendorRegisterService : IVendorRegisterService
    {
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly IEncryptionService _encryptionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorAttributeParser _vendorAttributeParser;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly IVendorService _vendorService;

        public VendorRegisterService(ICustomerService customerService, ILocalizationService localizationService,
            CustomerSettings customerSettings, IEncryptionService encryptionService, IVendorService vendorService,
            IRepository<CustomerRole> customerRoleRepository, IGenericAttributeService genericAttributeService,
            IVendorAttributeParser vendorAttributeParser, IVendorAttributeService vendorAttributeService)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _customerSettings = customerSettings;
            _encryptionService = encryptionService;
            _vendorService = vendorService;
            _customerRoleRepository = customerRoleRepository;
            _genericAttributeService = genericAttributeService;
            _vendorAttributeParser = vendorAttributeParser;
            _vendorAttributeService = vendorAttributeService;
        }

        public async Task<AddVendorResponse> AddVendorAsync(AddVendorModel model)
        {
            var validate = await ValidateAsync(model);
            if (validate != null)
                return new AddVendorResponse
                {
                    ErrorMessage = validate
                };

            var vendor = CreateVendor(model);
            await _vendorService.InsertVendorAsync(vendor);
            await SaveVendorAttributes(model.Attributes, vendor);

            var customer = new Customer
            {
                Active = true,
                Email = model.Email,
                Username = model.Email,
                VendorId = vendor.Id
            };
            await _customerService.InsertCustomerAsync(customer);
            await UpdateCustomerRoleMappingAsync(customer);
            await UpdatePasswordAsync(customer, PasswordFormat.Encrypted, model.Password);
            await SaveAttributeAsync(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
            await SaveAttributeAsync(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);
            await SaveAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);
            await SaveAttributeAsync(customer, NopCustomerDefaults.DateOfBirthAttribute, model.BirthDay);

            await SaveGenericAttributes(model.GenericAttributes, vendor);

            var addCustomerVendorResponse = new AddVendorResponse
            {
                VendorId = vendor.Id,
                CustomerId = customer.Id,
                UserName = model.Email,
                Email = model.Email,
                ErrorMessage = null
            };

            return addCustomerVendorResponse;
        }

        public async Task<AddVendorResponse> UpdateCustomerToVendorAsync(UpdateCustomerToVendorModel model)
        {
            var customer = model.Customer;

            var vendor = new Vendor
            {
                PictureId = model.PictureId,
                Email = string.IsNullOrEmpty(model.Email) ? customer.Email : model.Email,
                Description = model.Description,
                AdminComment = model.AdminComment,
                Name = string.IsNullOrEmpty(model.CompanyName) ? model.Email : model.CompanyName,
                Active = true
            };
            await _vendorService.InsertVendorAsync(vendor);

            customer.VendorId = vendor.Id;
            if (customer.Email.IsEmptyOrNull())
                customer.Email = model.Email;
            if (customer.Username.IsEmptyOrNull())
                customer.Username = model.Email;
            await _customerService.UpdateCustomerAsync(customer);

            await SaveVendorAttributes(model.Attributes, vendor);

            await UpdateCustomerRoleMappingAsync(customer);
            await SaveAttributeAsync(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
            await SaveAttributeAsync(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);
            await SaveAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);
            await SaveAttributeAsync(customer, NopCustomerDefaults.DateOfBirthAttribute, model.BirthDay);
            if (string.IsNullOrEmpty(model.Password) == false)
                await UpdatePasswordAsync(customer, PasswordFormat.Encrypted, model.Password);

            await SaveGenericAttributes(model.GenericAttributes, vendor);

            var response = new AddVendorResponse
            {
                VendorId = vendor.Id,
                CustomerId = customer.Id,
                UserName = model.Email,
                Email = model.Email,
                ErrorMessage = null
            };

            return response;
        }

        private async Task SaveGenericAttributes(Dictionary<string, string> genericAttributes, Vendor vendor)
        {
            if (genericAttributes != null)
                foreach (var item in genericAttributes)
                    await _genericAttributeService.SaveAttributeAsync(vendor, item.Key, item.Value);
        }

        private async Task SaveAttributeAsync<T>(Customer customer, string key, T data)
        {
            if (data == null)
                return;

            if (data is string d && string.IsNullOrEmpty(d))
                return;

            await _genericAttributeService.SaveAttributeAsync(customer, key, data);
        }

        private async Task SaveVendorAttributes(Dictionary<string, string> vendorAttributes, Vendor vendor)
        {
            if (vendorAttributes == null || !vendorAttributes.Any())
                return;

            var attributes = await _vendorAttributeService.GetAllVendorAttributesAsync();
            var attrsXml = string.Empty;

            foreach (var attrKey in vendorAttributes)
            {
                var vendorAttribute = attributes.FirstOrDefault(item => item.Name == attrKey.Key);
                if (vendorAttribute == null)
                    continue;

                attrsXml = _vendorAttributeParser.AddVendorAttribute(attrsXml, vendorAttribute, attrKey.Value);
            }

            await _genericAttributeService.SaveAttributeAsync(vendor, NopVendorDefaults.VendorAttributes, attrsXml);
        }

        private async Task<string> ValidateAsync(AddVendorModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
                return await _localizationService.GetResourceAsync("Account.Register.Errors.EmailIsNotProvided");

            if (!CommonHelper.IsValidEmail(model.Email))
                return await _localizationService.GetResourceAsync("Common.WrongEmail");

            if (string.IsNullOrWhiteSpace(model.Password))
                return await _localizationService.GetResourceAsync("Account.Register.Errors.PasswordIsNotProvided");

            if (await _customerService.GetCustomerByEmailAsync(model.Email) != null)
                return await _localizationService.GetResourceAsync("Account.Register.Errors.EmailAlreadyExists");

            if (await _customerService.GetCustomerByUsernameAsync(model.Email) != null)
                return await _localizationService.GetResourceAsync("Account.Register.Errors.UsernameAlreadyExists");

            return null;
        }

        private static Vendor CreateVendor(AddVendorModel model)
        {
            return new Vendor
            {
                PictureId = model.PictureId,
                Email = model.Email,
                Description = model.Description,
                Name = string.IsNullOrEmpty(model.CompanyName) ? model.Email : model.CompanyName,
                Active = true
            };
        }

        private async Task UpdateCustomerRoleMappingAsync(Customer customer)
        {
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            var queryVendorRoleIds = from customerRole in _customerRoleRepository.Table
                where (customerRole.SystemName == NopCustomerDefaults.VendorsRoleName ||
                       customerRole.SystemName == NopCustomerDefaults.RegisteredRoleName) &&
                      customerRoleIds.Contains(customerRole.Id) == false
                select customerRole;

            var roles = await queryVendorRoleIds.ToListAsync();

            foreach (var role in roles)
            {
                var customerRoleMapping = new CustomerCustomerRoleMapping
                {
                    CustomerId = customer.Id,
                    CustomerRoleId = role.Id
                };
                await _customerService.AddCustomerRoleMappingAsync(customerRoleMapping);
            }
        }

        private async Task UpdatePasswordAsync(Customer customer, PasswordFormat passwordFormat, string password)
        {
            var customerPassword = new CustomerPassword
            {
                CustomerId = customer.Id,
                PasswordFormat = passwordFormat,
                CreatedOnUtc = DateTime.UtcNow
            };

            switch (passwordFormat)
            {
                case PasswordFormat.Clear:
                    customerPassword.Password = password;
                    break;
                case PasswordFormat.Encrypted:
                    customerPassword.Password = _encryptionService.EncryptText(password);
                    break;
                case PasswordFormat.Hashed:
                    var saltKey = _encryptionService.CreateSaltKey(NopCustomerServicesDefaults.PasswordSaltKeySize);
                    customerPassword.PasswordSalt = saltKey;
                    customerPassword.Password = _encryptionService.CreatePasswordHash(password, saltKey,
                        _customerSettings.HashedPasswordFormat);
                    break;
            }

            await _customerService.InsertCustomerPasswordAsync(customerPassword);
        }
    }
}