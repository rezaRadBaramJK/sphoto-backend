using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Security;
using Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Models.Settings;
using Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Models.ViewModels.Settings;
using Nop.Plugin.Baramjk.ContactUs.Plugin;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Controllers
{
    [Permission(PermissionProvider.ManagementName)]
    [Area(AreaNames.Admin)]
    [Route("Admin/ContactUs/[action]")]
    public class PluginController: BaseBaramjkPluginAdminController<ContactUsSettings, ContactUsSettingsModel>
    {
        protected override PermissionRecord ConfigurePermissionRecord => PermissionProvider.ManagementRecord;
        
        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Areas/Admin/Views/{viewName}";
        }
        
    }
}