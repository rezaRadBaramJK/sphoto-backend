using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Services;
using Nop.Plugin.Baramjk.FrontendApi.Models;
using Nop.Plugin.Baramjk.FrontendApi.Models.Types;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class FrontendApiController : BasePluginController
    {
        #region Ctor

        public FrontendApiController(IJwtTokenService jwtTokenService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _jwtTokenService = jwtTokenService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #endregion

        #region Fields

        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        #endregion

        #region Methods

        public virtual async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //load settings for active store scope
            //  var webApiSettings = await _settingService.LoadSettingAsync<WebApiCommonSettings>();
            var webFrontApiSettings = await _settingService.LoadSettingAsync<FrontendApiSettings>();

            //prepare model
            var model = new ConfigurationModel
            {
                DeveloperMode = webFrontApiSettings.DeveloperMode,
                SecretKey = webFrontApiSettings.SecretKey,
                TokenLifetimeDays = webFrontApiSettings.TokenLifetimeDays,
                GoogleIOSClientId = webFrontApiSettings.GoogleIOSClientId,
                AppleIOSClientId = webFrontApiSettings.AppleIOSClientId,
                FaceBookIOSClientId = webFrontApiSettings.FaceBookIOSClientId,
                GoogleAndroidClientId = webFrontApiSettings.GoogleAndroidClientId,
                AppleAndroidClientId = webFrontApiSettings.AppleAndroidClientId,
                FaceBookAndroidClientId = webFrontApiSettings.FaceBookAndroidClientId,
                GoogleWebClientId = webFrontApiSettings.GoogleWebClientId,
                AppleWebClientId = webFrontApiSettings.AppleWebClientId,
                FaceBookWebClientId = webFrontApiSettings.FaceBookWebClientId,
                DontRequireAddress = webFrontApiSettings.DontRequireAddress,
                UploadFileSupportedTypes = webFrontApiSettings.UploadFileSupportedTypes,
                UploadFileMaxSize = webFrontApiSettings.UploadFileMaxSize,
                CustomFrontendBaseUrl = webFrontApiSettings.CustomFrontendBaseUrl,
                PasswordRecoveryStrategies =await Enum.GetValues<PasswordRecoveryStrategy>().SelectAwait(async s => new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedEnumAsync(s),
                    Value = ((int)s).ToString()
                }).ToListAsync(),
                PasswordRecoveryStrategy = (int) webFrontApiSettings.PasswordRecoveryStrategy
                
            };

            return View("~/Plugins/Baramjk.FrontendApi/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [ActionName("Configure")]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            //load settings for active store scope
            //var webApiSettings = await _settingService.LoadSettingAsync<WebApiCommonSettings>();
            var webFrontApiSettings = await _settingService.LoadSettingAsync<FrontendApiSettings>();

            //set settings
            webFrontApiSettings.DeveloperMode = model.DeveloperMode;
            webFrontApiSettings.SecretKey = model.SecretKey;
            webFrontApiSettings.TokenLifetimeDays = model.TokenLifetimeDays;
            webFrontApiSettings.TokenLifetimeMinutes = model.TokenLifetimeMinutes;
            webFrontApiSettings.GoogleIOSClientId = model.GoogleIOSClientId;
            webFrontApiSettings.AppleIOSClientId = model.AppleIOSClientId;
            webFrontApiSettings.FaceBookIOSClientId = model.FaceBookIOSClientId;
            webFrontApiSettings.GoogleAndroidClientId = model.GoogleAndroidClientId;
            webFrontApiSettings.AppleAndroidClientId = model.AppleAndroidClientId;
            webFrontApiSettings.FaceBookAndroidClientId = model.FaceBookAndroidClientId;
            webFrontApiSettings.GoogleWebClientId = model.GoogleWebClientId;
            webFrontApiSettings.AppleWebClientId = model.AppleWebClientId;
            webFrontApiSettings.FaceBookWebClientId = model.FaceBookWebClientId;
            webFrontApiSettings.DontRequireAddress = model.DontRequireAddress;
            webFrontApiSettings.UploadFileSupportedTypes = model.UploadFileSupportedTypes;
            webFrontApiSettings.UploadFileMaxSize = model.UploadFileMaxSize;
            webFrontApiSettings.CustomFrontendBaseUrl = model.CustomFrontendBaseUrl;
            webFrontApiSettings.PasswordRecoveryStrategy = (PasswordRecoveryStrategy) model.PasswordRecoveryStrategy;
            
            await _settingService.SaveSettingAsync(webFrontApiSettings,
                settings => settings.DeveloperMode, clearCache: false);
            await _settingService.SaveSettingAsync(webFrontApiSettings,
                settings => settings.SecretKey, clearCache: false);

            await _settingService.SaveSettingAsync(webFrontApiSettings);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        [HttpPost]
        public virtual IActionResult Generate()
        {
            return Ok(_jwtTokenService.NewSecretKey);
        }

        #endregion
    }
}