using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Plugin.Baramjk.BackendApi.Framework;
using Nop.Plugin.Baramjk.BackendApi.Framework.Services;
using Nop.Plugin.Baramjk.BackendApi.Services.Security;
using Nop.Plugin.Baramjk.Framework.Nop.Plugins;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Baramjk.BackendApi
{
    /// <summary>
    ///     Represents the Web API Backend plugin
    /// </summary>
    public class WebApiBackendPlugin : BaramjkPlugin, IMiscPlugin, IAdminMenuPlugin
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<PermissionRecord> _permissionRecordRepository;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public WebApiBackendPlugin(IActionContextAccessor actionContextAccessor, IJwtTokenService jwtTokenService,
            ILocalizationService localizationService, IPermissionService permissionService,
            IRepository<PermissionRecord> permissionRecordRepository, ISettingService settingService,
            IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _jwtTokenService = jwtTokenService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _permissionRecordRepository = permissionRecordRepository;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
        }

        #region Methods

        /// <summary>
        ///     Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext)
                .RouteUrl(WebApiBackendDefaults.ConfigurationRouteName);
        }

        /// <summary>
        ///     Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //locales
            await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Baramjk.BackendApi.CorsOrigins"] = "Cors Origins",
                ["Baramjk.BackendApi.DeveloperMode"] = "Developer mode",
                ["Baramjk.BackendApi.DeveloperMode.Hint"] =
                    "Developer mode allows you to make requests without using JWT.",
                ["Baramjk.BackendApi.SecretKey"] = "Secret key",
                ["Baramjk.BackendApi.SecretKey.Generate"] = "Generate new",
                ["Baramjk.BackendApi.SecretKey.Hint"] = "The secret key to sign and verify each JWT token."
            });

            //settings
            await _settingService.SaveSettingAsync(new WebApiBackendSettings
            {
                TokenLifetimeDays = WebApiCommonDefaults.TokenLifeTime,
                SecretKey = _jwtTokenService.NewSecretKey
            });

            //add permission
            await _permissionService.InstallPermissionsAsync(
                (IPermissionProvider)Activator.CreateInstance(typeof(WebApiBackendPermissionProvider)));

            await base.InstallAsync();
        }

        /// <summary>
        ///     Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Baramjk.BackendApi");

            //settings
            await _settingService.DeleteSettingAsync<WebApiBackendSettings>();

            //delete permission
            var permissionRecord = (await _permissionService.GetAllPermissionRecordsAsync())
                .FirstOrDefault(x => x.SystemName == WebApiBackendPermissionProvider.AccessWebApiBackend.SystemName);
            var listMappingCustomerRolePermissionRecord =
                await _permissionService.GetMappingByPermissionRecordIdAsync(permissionRecord.Id);
            foreach (var mappingCustomerPermissionRecord in listMappingCustomerRolePermissionRecord)
                await _permissionService.DeletePermissionRecordCustomerRoleMappingAsync(
                    mappingCustomerPermissionRecord.PermissionRecordId,
                    mappingCustomerPermissionRecord.CustomerRoleId);

            await _permissionRecordRepository.DeleteAsync(permissionRecord);

            await base.UninstallAsync();
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            decimal version;
            try
            {
                version = Convert.ToDecimal(currentVersion, CultureInfo.InvariantCulture);
            }
            catch
            {
                return;
            }

            if (version < 1.04M)
                await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
                {
                    ["Plugins.WebApi.Backend.SecretKey.Generate"] = "Generate new"
                });
        }

        private static SiteMapNode _pluginSiteMapNode;

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (await _permissionService.AuthorizeAsync(WebApiBackendPermissionProvider.AccessWebApiBackend) == false)
                return;

            if (_pluginSiteMapNode == null)
            {
                var nodes = new SiteMapNode[] { };

                _pluginSiteMapNode = CreatePluginSiteMapNode(FriendlyName, nodes.ToArray());
            }

            rootNode.AddToBaramjkPluginsMenu(_pluginSiteMapNode);
        }

        #endregion
    }
}