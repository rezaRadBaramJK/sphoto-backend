using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Nop.Plugins;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Plugin.Baramjk.FrontendApi.Framework;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Services;
using Nop.Plugin.Baramjk.FrontendApi.Infrastructure;
using Nop.Plugin.Baramjk.FrontendApi.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

#pragma warning disable CS1998

namespace Nop.Plugin.Baramjk.FrontendApi
{
    public class FrontendApiPlugin : BaramjkPlugin, IMiscPlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly FrontendApiMessageTemplateService _frontendApiMessageTemplateService;

        public FrontendApiPlugin(
            IJwtTokenService jwtTokenService,
            ILocalizationService localizationService,
            ISettingService settingService,
            FrontendApiMessageTemplateService frontendApiMessageTemplateService)
        {
            _jwtTokenService = jwtTokenService;
            _localizationService = localizationService;
            _settingService = settingService;
            _frontendApiMessageTemplateService = frontendApiMessageTemplateService;
        }

        public override string GetConfigurationPageUrl()
        {
            return "/Admin/FrontendApi/Configure";
        }

        public override async Task InstallAsync()
        {
            //locales
            await _localizationService.AddLocaleResourceAsync(Localization);

            var webApiFrontendSettings = new FrontendApiSettings
            {
                TokenLifetimeDays = WebApiCommonDefaults.TokenLifeTime,
                SecretKey = _jwtTokenService.NewSecretKey
            };

            await _settingService.SaveSettingAsync(webApiFrontendSettings);
            await _frontendApiMessageTemplateService.InitTemplatesAsync();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _localizationService.DeleteLocaleResourcesAsync("Baramjk.FrontendApi");
            await _settingService.DeleteSettingAsync<FrontendApiSettings>();
            await _frontendApiMessageTemplateService.DeleteCustomerNewPasswordRecoveryMessageTemplateAsync();
            await base.UninstallAsync();
        }
        
        private static SiteMapNode _cachePluginSiteMapNode;

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (await AuthorizeAsync(PermissionProvider.ManagementRecord) == false)
                return;
            
            if (_cachePluginSiteMapNode == null)
            {
               
                _cachePluginSiteMapNode = CreatePluginSiteMapNode(FriendlyName);
            }

            rootNode.AddToBaramjkPluginsMenu(_cachePluginSiteMapNode);
        }

        public bool HideInWidgetList => false;

        public async Task<IList<string>> GetWidgetZonesAsync()
        {
            return new List<string>
            {
                PublicWidgetZones.ProductDetailsBottom
            };
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "WidgetsProductVisit";
        }


        public static readonly Dictionary<string, string> Localization = new()
        {
            ["Baramjk.FrontendApi.DeveloperMode"] = "Developer mode",
            ["Baramjk.FrontendApi.DeveloperMode.Hint"] = "Developer mode allows you to make requests without using JWT.",
            ["Baramjk.FrontendApi.SecretKey"] = "Secret key",
            ["Baramjk.FrontendApi.SecretKey.Hint"] = "The secret key to sign and verify each JWT token.",
            ["Baramjk.FrontendApi.Settings.UploadFileSupportedTypes"] = "Upload File Supported Types",
            ["Baramjk.FrontendApi.Settings.UploadFileMaxSize"] = "Upload File Max Size (MB)",
            ["Baramjk.FrontendApi.Settings.CustomFrontendBaseUrl"] = "Custom Frontend Base Url",
            ["Baramjk.FrontendApi.Settings.SecretKey.Generate"] = "Generate",
            ["Plugins.WebApi.Frontend.SecretKey"] = "Secret Key",
            ["Plugins.WebApi.Frontend.DeveloperMode"] = "Developer Mode",
            ["Baramjk.FrontendApi.Order.ReOrder.Message"] = "Some products from your previous order aren’t available right now.\nOnly the available items have been added to your cart.",
            ["Baramjk.FrontendApi.Settings.PasswordRecoveryStrategy"] = "Password Recovery Strategy",
        };
    }
}