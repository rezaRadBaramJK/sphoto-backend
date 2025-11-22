using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Baramjk.Banner.Controllers.Api.Backend.Abstractions;
using Nop.Plugin.Baramjk.Banner.Services;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Services.Customers;
using Nop.Services.Vendors;

namespace Nop.Plugin.Baramjk.Banner.Controllers.Api.Backend
{
    public class VendorApiController : BaseAdminVendorBackendApiController
    {
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;
        private readonly BannerVendorService _bannerVendorService;
        private readonly BannerService _bannerService;

        public VendorApiController(IWorkContext workContext, 
            ICustomerService customerService,
            IVendorService vendorService,
            BannerVendorService bannerVendorService,
            BannerService bannerService)
        {
            _workContext = workContext;
            _customerService = customerService;
            _vendorService = vendorService;
            _bannerVendorService = bannerVendorService;
            _bannerService = bannerService;
        }


        [HttpPost("/BackendApi/Banner/Vendor")]
        public async Task<IActionResult> UploadLogoAndBannersAsync([FromForm] int vendorId)
        {
            if (vendorId <= 0)
                return ApiResponseFactory.BadRequest("Invalid vendor id");

            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (vendor == null)
                return ApiResponseFactory.BadRequest("Vendor not found.");

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsVendorAsync(currentCustomer))
            {
                var currentVendor = await _workContext.GetCurrentVendorAsync();
                
                if (currentVendor == null ||
                    currentVendor.Active == false ||
                    currentVendor.Deleted ||
                    currentVendor.Id != vendorId)
                    return AccessDenied();
            }
            else if (await _customerService.IsAdminAsync(currentCustomer) == false)
                return AccessDenied();
            
            var logo = Request.Form.Files.GetFile("logo");
            if (logo != null)
                await _bannerVendorService.SaveLogoAsync(logo, vendor);
            
            var banners = Request.Form.Files.GetFiles("Banner").ToList();
            if (banners.Any())
                await _bannerVendorService.SaveBannersAsync(banners, vendor);
            return ApiResponseFactory.Success();
        }

        [HttpDelete("/BackendApi/Banner/Vendor/{vendorId:int}/Logo")]
        public async Task<IActionResult> DeleteLogoAsync(int vendorId)
        {
            if (vendorId <= 0)
                return ApiResponseFactory.BadRequest("Invalid vendor id");

            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (vendor == null)
                return ApiResponseFactory.BadRequest("Vendor not found.");
            
            
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsVendorAsync(currentCustomer))
            {
                var currentVendor = await _workContext.GetCurrentVendorAsync();
                
                if (currentVendor == null ||
                    currentVendor.Active == false ||
                    currentVendor.Deleted ||
                    currentVendor.Id != vendorId)
                    return AccessDenied();
            }
            else if (await _customerService.IsAdminAsync(currentCustomer) == false)
                return AccessDenied();
            
            await _bannerVendorService.RemoveLogoAsync(vendor);
            return ApiResponseFactory.Success();
        }

        [HttpDelete("/BackendApi/Banner/Vendor/{vendorId:int}/Banner/{bannerId:int}")]
        public async Task<IActionResult> DeleteBannerAsync(int vendorId, int bannerId)
        {
            if (vendorId <= 0)
                return ApiResponseFactory.BadRequest("Invalid vendor id");

            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (vendor == null)
                return ApiResponseFactory.BadRequest("Vendor not found.");

            var banner = await _bannerService.GetByIdAsync(bannerId);
            if (banner == null)
                return ApiResponseFactory.BadRequest("Banner not found.");

            if (banner.EntityName != nameof(Vendor) || banner.EntityId != vendorId)
                return AccessDenied();
            
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsVendorAsync(currentCustomer))
            {
                var currentVendor = await _workContext.GetCurrentVendorAsync();
                
                if (currentVendor == null ||
                    currentVendor.Active == false ||
                    currentVendor.Deleted ||
                    currentVendor.Id != vendorId)
                    return AccessDenied();
            }
            else if (await _customerService.IsAdminAsync(currentCustomer) == false)
                return AccessDenied();

            await _bannerService.DeleteAsync(banner);
            return ApiResponseFactory.Success();
        }
        
    }
}