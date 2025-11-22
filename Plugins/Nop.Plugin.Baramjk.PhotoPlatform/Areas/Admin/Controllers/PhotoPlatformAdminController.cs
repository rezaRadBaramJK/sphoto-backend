using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Security;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    [Route("Admin/PhotoPlatformAdmin/[action]")]
    public class PhotoPlatformAdminController : BaseBaramjkPluginAdminController<PhotoPlatformSettings, PhotoPlatformSettingsModel>
    {
        protected override PermissionRecord ConfigurePermissionRecord => PermissionProvider.ManagementRecord;

        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Areas/Admin/Views/{viewName}";
        }
    }
}