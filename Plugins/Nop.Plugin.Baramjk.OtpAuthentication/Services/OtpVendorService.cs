using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Baramjk.OtpAuthentication.Services.Abstractions;
using Nop.Services.Customers;
using Nop.Services.Vendors;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Services
{
    public class OtpVendorService : IOtpVendorService
    {
        private readonly ICustomerService _customerService;
        private readonly VendorSettings _vendorSettings;
        private readonly IVendorService _vendorService;

        public OtpVendorService(ICustomerService customerService, VendorSettings vendorSettings, IVendorService vendorService)
        {
            _customerService = customerService;
            _vendorSettings = vendorSettings;
            _vendorService = vendorService;
        }

        public async Task RegisterCustomerAsVendorAsync(Customer customer)
        {

            var vendorRole =
                await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.VendorsRoleName);
            if (vendorRole == null)
            {
                return;
            }
            
            
            //disabled by default
            var vendor = new Vendor
            {
                Name = customer.Email,
                Email = customer.Email,
                //some default settings
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = _vendorSettings.DefaultVendorPageSizeOptions,
                Description = "",
                Active = true
            };

            await _vendorService.InsertVendorAsync(vendor);

            customer.VendorId = vendor.Id;
            await _customerService.UpdateCustomerAsync(customer);
            
            await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping
            {
                CustomerId = customer.Id,
                CustomerRoleId = vendorRole.Id
            });
        }
    }
}