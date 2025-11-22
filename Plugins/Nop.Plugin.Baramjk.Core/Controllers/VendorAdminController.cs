using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.Vendors;
using Nop.Plugin.Baramjk.Framework.Services.Vendors.Models;
using Nop.Services.Vendors;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.Core.Controllers
{
    [Area(AreaNames.Admin)]
    public class VendorAdminController: BaseBaramjkPluginController
    {
        private readonly IVendorService _vendorService;
        private readonly VendorDetailsService _vendorDetailsService;

        public VendorAdminController(
            IVendorService vendorService,
            VendorDetailsService vendorDetailsService)
        {
            _vendorService = vendorService;
            _vendorDetailsService = vendorDetailsService;
        }


        [HttpPost("Admin/Core/Vendor/{vendorId:int}")]
        public async Task<IActionResult> SubmitDetailsAsync(
            [FromForm] SubmitVendorDetailsParams submitParams,
            [FromRoute] int vendorId)
        {
            if (vendorId <= 0)
                return ApiResponseFactory.BadRequest("Invalid vendor id.");
            
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (vendor == null)
                return ApiResponseFactory.BadRequest("Vendor not found.");

            submitParams.Vendor = vendor;
            await _vendorDetailsService.SubmitVendorDetailsAsync(submitParams);
            return ApiResponseFactory.Success();

        }
    }
}