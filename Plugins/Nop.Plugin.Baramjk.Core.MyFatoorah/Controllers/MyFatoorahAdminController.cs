using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Security;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Factories;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Controllers
{
    [Permission(PermissionProvider.MyFatoorahManagement)]
    [Area(AreaNames.Admin)]
    public class MyFatoorahAdminController :
        BaseBaramjkPluginAdminController<MyFatoorahSettings, MyFatoorahSettingModel>
    {
        private readonly MyFatoorahSettings _myFatoorahSettings;
        private readonly SupplierAdminFactory _supplierAdminFactory;

        public MyFatoorahAdminController(
            MyFatoorahSettings myFatoorahSettings,
            SupplierAdminFactory supplierAdminFactory)
        {
            _myFatoorahSettings = myFatoorahSettings;
            _supplierAdminFactory = supplierAdminFactory;
        }

        protected override PermissionRecord ConfigurePermissionRecord => PermissionProvider.ManagementRecord;
        
        public override async Task<IActionResult> Configure()
        {
            if (ConfigurePermissionRecord != null &&
                !await _permissionService.AuthorizeAsync(ConfigurePermissionRecord))
                return AccessDeniedView();
            
            var model = MapUtils.Map<MyFatoorahSettingModel>(_myFatoorahSettings);
            model.AvailableSuppliers = await _supplierAdminFactory.PrepareAvailableSuppliersAsync();
            return View("Configure.cshtml", model);
        }
        
    }
}