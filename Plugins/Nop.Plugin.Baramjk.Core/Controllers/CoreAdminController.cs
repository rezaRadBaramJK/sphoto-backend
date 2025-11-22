using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Security;
using Nop.Plugin.Baramjk.Core.Models;
using Nop.Plugin.Baramjk.Core.Plugins;
using Nop.Plugin.Baramjk.Framework;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.Core.Controllers
{
    [Area(AreaNames.Admin)]
    public class CoreAdminController : BaseBaramjkPluginAdminController<FrameworkSettings, FrameworkSettingsModel>
    {
        protected override PermissionRecord ConfigurePermissionRecord => PermissionProvider.Management;
    }
}