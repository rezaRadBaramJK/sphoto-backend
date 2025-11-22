using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Core.Factories;
using Nop.Plugin.Baramjk.Framework.Mvc.ViewComponents;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Baramjk.Core.Components
{
    [ViewComponent(Name = "BaramjkCoreWidgets")]
    public class WidgetsViewComponent: BaramjkViewComponent
    {
        private readonly VendorAdminFactory _vendorAdminFactory;
        private readonly ILogger _logger;
        private readonly ICustomerService _customerService;

        public WidgetsViewComponent(
            VendorAdminFactory vendorAdminFactory,
            ILogger logger, ICustomerService customerService)
        {
            _vendorAdminFactory = vendorAdminFactory;
            _logger = logger;
            _customerService = customerService;
        }
        
        public Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (widgetZone == AdminWidgetZones.VendorDetailsBlock)
            {
                return InvokeVendorDetailsBlockAsync(additionalData);
            }
            return Task.FromResult<IViewComponentResult>(Content(string.Empty));
        }

        private async Task<IViewComponentResult> InvokeVendorDetailsBlockAsync(object additionalData)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors, customer) == false)
                return Content(string.Empty);
            
            if (additionalData is not VendorModel vendorModel)
            {
                await _logger.ErrorAsync("Core - WidgetsViewComponent: Invalid vendor model.");
                return Content(string.Empty);
            }
            
            if (await _customerService.IsVendorAsync(customer))
            {
                var vendor = await _workContext.GetCurrentVendorAsync();
                if (vendor == null || vendor.Deleted || vendor.Active == false || vendor.Id != vendorModel.Id)
                    return Content(string.Empty);
            }

            if (vendorModel.Id == 0)
                return Content(string.Empty);
                
            var vendorBranchViewModel = await _vendorAdminFactory.PrepareVendorBranchViewModelAsync(vendorModel.Id);
            return ViewBase($"~/Plugins/{SystemName}/Views/Widgets/Vendors/VendorDetailsBlock.cshtml", vendorBranchViewModel);
        }
    }
}