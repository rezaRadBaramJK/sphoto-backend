using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Security;
using Nop.Plugin.Baramjk.Banner.Models;
using Nop.Plugin.Baramjk.Banner.Plugins;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.Banner.Controllers.Admin
{
    [Permission(PermissionProvider.ManagementName)]
    [Area(AreaNames.Admin)]
    public class BannerAdminController : BaseBaramjkPluginAdminController<BannerSettings,BannerSettingsModel>
    {
        protected override PermissionRecord ConfigurePermissionRecord => PermissionProvider.Management;

        protected override string SystemName => DefaultValue.SystemName;
    }
}