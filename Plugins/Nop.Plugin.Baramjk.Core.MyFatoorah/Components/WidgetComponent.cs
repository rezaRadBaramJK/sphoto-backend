using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Factories;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Components
{
    [ViewComponent(Name = "MyFatoorahWidgets")]
    public class WidgetComponent : NopViewComponent
    {
        private readonly SupplierAdminFactory _supplierAdminFactory;
        private readonly IPermissionService _permissionService;

        public WidgetComponent(
            SupplierAdminFactory supplierAdminFactory,
            IPermissionService permissionService)
        {
            _supplierAdminFactory = supplierAdminFactory;
            _permissionService = permissionService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (await _permissionService.AuthorizeAsync(PermissionProvider.ManagementRecord) == false)
                return Content(string.Empty);
            
            if (widgetZone == AdminWidgetZones.ProductDetailsBlock)
                return await InvokeProductDetailsBlockAsync(additionalData);
            
            return Content(string.Empty);
        }

        private async Task<IViewComponentResult> InvokeProductDetailsBlockAsync(object additionalData)
        {
            if (additionalData is not ProductModel productModel || productModel.Id == 0)
                return Content(string.Empty);

            var viewModel = await _supplierAdminFactory.PrepareProductSupplierAsync(productModel.Id);
            return View("~/Plugins/Baramjk.Core.MyFatoorah/Views/Widgets/ProductSupplier.cshtml", viewModel);
        }
    }
}