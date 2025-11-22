using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.Framework.Services.Sms;
using Nop.Plugin.Baramjk.PushNotification.Domain;
using Nop.Plugin.Baramjk.PushNotification.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.PushNotification.Controllers
{
    [Permission(PermissionProvider.PushNotificationManagementName)]
    [Area(AreaNames.Admin)]
    public class SmsAdminController : BaseBaramjkPluginAdminController
    {
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ISmsService _smsService;
        public SmsAdminController(ISettingService settingService, INotificationService notificationService, ISmsService smsService)
        {
            _settingService = settingService;
            _notificationService = notificationService;
            _smsService = smsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSmsConfiguration()
        {
            var setting = await _settingService.LoadSettingAsync<SmsProviderSetting>();
            return View("Menu/Sms/Configure.cshtml", setting);
        }
        [HttpGet]
        public async Task<IActionResult> SendManualSms()
        {
            return View("Menu/Sms/Manual.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> DoSendManualSms(ManualSendSmsModel model)
        {
            await _smsService.SendSms(receptor: model.Receptor, message: model.Text);
            return View("Menu/Sms/Manual.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSmsConfiguration(SmsProviderSetting model)
        {
            try
            {
                await _settingService.SaveSettingAsync(model);
                _notificationService.SuccessNotification("SMS provider setting updated");
            }
            catch (Exception e)
            {
                _notificationService.ErrorNotification("error while updating SMS provider setting ");
            }
            
            return RedirectToAction("GetSmsConfiguration");
        }
    }
}