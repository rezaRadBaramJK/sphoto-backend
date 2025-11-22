using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Plugin.Baramjk.Framework.Models.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Media;

namespace Nop.Plugin.Baramjk.Framework.Factories
{
    public class CustomerDtoFactory : ICustomerDtoFactory
    {
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly MediaSettings _mediaSettings;
        private readonly IPictureService _pictureService;
        private readonly IAddressService _addressService;

        public CustomerDtoFactory(IPictureService pictureService, IGenericAttributeService genericAttributeService,
            MediaSettings mediaSettings, ICustomerService customerService, IAddressService addressService)
        {
            _pictureService = pictureService;
            _genericAttributeService = genericAttributeService;
            _mediaSettings = mediaSettings;
            _customerService = customerService;
            _addressService = addressService;
        }

        public virtual async Task<CustomerDto> PrepareCustomerDtoAsync(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            var customerModel = await PrepareCustomerDtoAsync(customer);
            return customerModel;
        }

        public virtual async Task<CustomerDto> PrepareCustomerDtoAsync(Customer customer)
        {
            var model = new CustomerDto();

            if (customer == null)
                return model;

            model.Id = customer.Id;
            model.Email = customer.Email;
            model.Username = customer.Username;

            model.FirstName = await
                _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute);
            model.LastName = await
                _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute);
            model.Gender = await
                _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.GenderAttribute);
            model.Phone = await
                _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute);
            var dateOfBirth = await _genericAttributeService.GetAttributeAsync<DateTime?>(customer,
                NopCustomerDefaults.DateOfBirthAttribute);

            if (dateOfBirth.HasValue)
            {
                model.DateOfBirthDay = dateOfBirth.Value.Day;
                model.DateOfBirthMonth = dateOfBirth.Value.Month;
                model.DateOfBirthYear = dateOfBirth.Value.Year;
            }

            var avatarPictureIdAttribute = await _genericAttributeService.GetAttributeAsync<int>(customer,
                NopCustomerDefaults.AvatarPictureIdAttribute);

            model.AvatarUrl = await _pictureService.GetPictureUrlAsync(avatarPictureIdAttribute,
                _mediaSettings.AvatarPictureSize, false);

            return model;
        }

        public virtual async Task<CustomerDto> PrepareCustomerDtoAsync(int customerId, int addressId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            var customerModel = await PrepareCustomerDtoAsync(customer, addressId);
            return customerModel;
        }

        public virtual async Task<CustomerDto> PrepareCustomerDtoAsync(Customer customer, int addressId)
        {
            var address = await _addressService.GetAddressByIdAsync(addressId);
            var avatarPictureIdAttribute = await _genericAttributeService.GetAttributeAsync<int>(customer,
                NopCustomerDefaults.AvatarPictureIdAttribute);

            var model = new CustomerDto
            {
                Email = address.Email,
                Id = customer.Id,
                Phone = address.PhoneNumber,
                Username = customer.Username,
                FirstName = address.FirstName,
                LastName = address.LastName,
                Gender = await
                    _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.GenderAttribute),
                AvatarUrl = await _pictureService.GetPictureUrlAsync(avatarPictureIdAttribute,
                    _mediaSettings.AvatarPictureSize, false),
            };

            var dateOfBirth = await _genericAttributeService.GetAttributeAsync<DateTime?>(customer,
                NopCustomerDefaults.DateOfBirthAttribute);
            if (dateOfBirth.HasValue)
            {
                model.DateOfBirthDay = dateOfBirth.Value.Day;
                model.DateOfBirthMonth = dateOfBirth.Value.Month;
                model.DateOfBirthYear = dateOfBirth.Value.Year;
            }

            return model;
        }
    }
}