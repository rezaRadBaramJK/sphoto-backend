using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework;
using Nop.Plugin.Baramjk.Framework.Nop.Plugins;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

#pragma warning disable CS1998

namespace Nop.Plugin.Baramjk.Core.Plugins
{
    public class BaramjkCorePlugin : BaramjkPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        private readonly ISettingService _settingService;

        public BaramjkCorePlugin(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public static FrameworkSettings GetDefaultFrameworkSetting => new();

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(GetDefaultFrameworkSetting);
            await PermissionService.InstallPermissionsAsync(new PermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await base.UninstallAsync();
        }

        private static SiteMapNode _cachePluginSiteMapNode;

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (!(await AuthorizeAsync(PermissionProvider.Management)))
                return;
            
            MenuUtils.GetBaramjkMenu(rootNode);

            _cachePluginSiteMapNode ??= CreatePluginSiteMapNode(FriendlyName);

            rootNode.AddToBaramjkPluginsMenu(_cachePluginSiteMapNode);
        }


        public static Dictionary<string, string> Localizations => new()
        {
            {"Nop.Plugin.Baramjk.Core.Admin.Vendors.Details.Titles", "Details"},
            {"Nop.Plugin.Baramjk.Core.Admin.Vendors.Details.StartTime", "Start Time"},
            {"Nop.Plugin.Baramjk.Core.Admin.Vendors.Details.EndTime", "End Time"},
            {"Nop.Plugin.Baramjk.Core.Admin.Vendors.Details.OffDaysOfWeekIds", "Off Days Of Week"},
            {"Nop.Plugin.Baramjk.Core.Admin.Vendors.Details.IsAvailable", "Is Available"},
            {"Nop.Plugin.Baramjk.Core.Admin.Vendors.Details.SaveDetails", "Save Details"},
        };

        public bool HideInWidgetList => false;
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                AdminWidgetZones.VendorDetailsBlock
            });
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "BaramjkCoreWidgets";
        }
    }
}