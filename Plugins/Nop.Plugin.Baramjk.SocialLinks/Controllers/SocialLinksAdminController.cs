using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Security;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.SocialLinks.Models;
using Nop.Plugin.Baramjk.SocialLinks.Plugins;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.SocialLinks.Controllers
{
    [Permission(PermissionProvider.SocialLinksName)]
    [Area(AreaNames.Admin)]
    public class SocialLinksAdminController
        : BaseBaramjkPluginAdminController<SocialLinksSettings, SocialLinksSettingsModel>
    {
        protected override string SystemName => DefaultValue.SystemName;
        protected override PermissionRecord ConfigurePermissionRecord => PermissionProvider.SocialLinksManagement;
    }
}