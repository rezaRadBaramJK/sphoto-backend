using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Security;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.PushNotification.Factories.Admin;
using Nop.Plugin.Baramjk.PushNotification.Models;
using Nop.Plugin.Baramjk.PushNotification.Plugins;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.PushNotification.Controllers
{
    [Permission(PermissionProvider.PushNotificationManagementName)]
    [Area(AreaNames.Admin)]
    public class PushNotificationAdminController
        : BaseBaramjkPluginAdminController<PushNotificationSettings, PushNotificationSettingModel>
    {
        private readonly PushNotificationSettings _pushNotificationSettings;
        private readonly SmsProviderSetting _smsProviderSetting;
        private readonly SettingFactory _settingFactory;

        public PushNotificationAdminController(
            PushNotificationSettings pushNotificationSettings,
            SmsProviderSetting smsProviderSetting, 
            SettingFactory settingFactory)
        {
            _pushNotificationSettings = pushNotificationSettings;
            _smsProviderSetting = smsProviderSetting;
            _settingFactory = settingFactory;
        }

        public override async Task<IActionResult> Configure()
        {
            
            var pushMode = new PushNotificationSettingModel
            {
                ServerKey = _pushNotificationSettings.ServerKey,
                FireBaseConfig = _pushNotificationSettings.FireBaseConfig,
                PrivateKeyConfig = _pushNotificationSettings.PrivateKeyConfig,
                SoundFileName = _pushNotificationSettings.SoundFileName,
                Strategy = _pushNotificationSettings.Strategy,
                DisableNotification = _pushNotificationSettings.DisableNotification,
                WhatsAppSettings = _settingFactory.PreparedWhatsAppSettingViewModel(),
                StrategiesListItems = new List<SelectListItem>
                {
                    new()
                    {
                        Text = await _localizationService.GetResourceAsync("Baramjk.PushNotification.FcmHttpV1"),
                        Value = "1",
                        Selected = _pushNotificationSettings.Strategy == 1 
                    },
                    new()
                    {
                        Text = await _localizationService.GetResourceAsync("Baramjk.PushNotification.FcmLegacy"),
                        Value = "2",
                        Selected = _pushNotificationSettings.Strategy == 2
                    }
                }
            };
            ViewBag.sms = _smsProviderSetting;
            return View("Configure.cshtml", pushMode);
        }

        protected override bool ValidateConfigureModel(PushNotificationSettingModel model)
        {
            model.ServerKey = model.ServerKey.TrimSafe();
            return ModelState.IsValid;
        }
        
        protected override PermissionRecord ConfigurePermissionRecord => PermissionProvider.Management;
        protected override string SystemName => DefaultValue.SystemName;
    }
}