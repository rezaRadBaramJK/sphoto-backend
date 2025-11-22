using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Nop.Plugins;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Settings;
using Nop.Plugin.Baramjk.OtpAuthentication.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Plugin
{
    public class OtpAuthenticationPlugin : BaramjkPlugin, IAdminMenuPlugin
    {
        private static SiteMapNode _cachePluginSiteMapNode;
        
        public static OtpAuthenticationSettings DefaultSetting => new()
        {
            Message = "",
            TokenLifetimeSeconds = 0,
            AllowRegisterationAsVendor = false
        };
        
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly OtpMessageTemplateService _otpMessageTemplateService;
        private readonly IPermissionService _permissionService;

        public OtpAuthenticationPlugin(
            ISettingService settingService, 
            ILocalizationService localizationService,
            OtpMessageTemplateService otpMessageTemplateService,
            IPermissionService permissionService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _otpMessageTemplateService = otpMessageTemplateService;
            _permissionService = permissionService;
        }

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new OtpAuthenticationSettings());
            await _localizationService.AddLocaleResourceAsync(LocalizationResources);
            await _otpMessageTemplateService.AddOtpMessageTemplateIfNotExistAsync();
            await _permissionService.InstallPermissionsAsync(new PermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _otpMessageTemplateService.DeleteOtpMessageTemplateAsync();
            await _settingService.DeleteSettingAsync<OtpAuthenticationSettings>();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (await AuthorizeAsync(PermissionProvider.Management) == false)
                return;
            
            _cachePluginSiteMapNode ??= CreatePluginSiteMapNode(FriendlyName);
            rootNode.AddToBaramjkPluginsMenu(_cachePluginSiteMapNode);
        }

        public static Dictionary<string, string> LocalizationResources => new()
        {
            {"Nop.Plugin.Baramjk.OtpAuthentication.Admin.Configuration.SendMethod", "Send Method"},
            {"Nop.Plugin.Baramjk.OtpAuthentication.Admin.Configuration.WhatsAppUsername", "What's app Username"},
            {"Nop.Plugin.Baramjk.OtpAuthentication.Admin.Configuration.WhatsAppPassword", "What's app Password"},
        };
    }
}