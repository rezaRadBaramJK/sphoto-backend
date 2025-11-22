using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Plugin.Baramjk.Framework.Nop.Plugins;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Configuration;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Baramjk.LocationDetector.Plugin
{
    public class LocationDetectorPlugin : BaramjkPlugin, IAdminMenuPlugin
    {
        public virtual string FriendlyName => PluginDescriptor.FriendlyName;
        protected static SiteMapNode _pluginSiteMapNode;
        private readonly ISettingService _settingService;

        public LocationDetectorPlugin(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public static LocationDetectorSettings GetDefaultSetting => new()
        {
            DefaultCurrency = "KWD",
            Ip2LocationToken = "D81D469C9DCC3E60339CF4FEC1B52B60",
        };

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (_pluginSiteMapNode == null)
            {
                var locationDetector = CreateSiteMapNode("LocationDetectorAdmin", "List", "Currency Mapping");

                _pluginSiteMapNode =
                    CreatePluginSiteMapNode(FriendlyName, locationDetector);
            }

            rootNode.AddToBaramjkPluginsMenu(_pluginSiteMapNode);
        }

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(GetDefaultSetting);

            await PermissionService.InstallPermissionsAsync(new PermissionProvider());
            await base.InstallAsync();
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // throw new System.NotImplementedException();
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{WebHelper.GetStoreLocation()}Admin/LocationDetectorConfigurationAdmin/Configure";
        }

        public int Order => 1002;
    }
}