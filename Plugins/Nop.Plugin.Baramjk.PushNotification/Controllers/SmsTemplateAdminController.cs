using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
    public class SmsTemplateAdminController : BaseBaramjkPluginAdminController
    {
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ISmsService _smsService;
        public SmsTemplateAdminController(ISettingService settingService, INotificationService notificationService, ISmsService smsService)
        {
            _settingService = settingService;
            _notificationService = notificationService;
            _smsService = smsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSmsTemplateConfiguration()
        {
            var setting = await _settingService.LoadSettingAsync<SmsTemplateSetting>();
            return View("Menu/SmsTemplate/Configure.cshtml", setting);
        }
        
        
        [HttpPost]
        public async Task<IActionResult> UpdateSmsTemplateConfiguration(SmsTemplateSetting model)
        {
            try
            {
                await _settingService.SaveSettingAsync(model);
                _notificationService.SuccessNotification("SMS template setting updated");
            }
            catch (Exception e)
            {
                _notificationService.ErrorNotification("error while updating SMS template setting ");
            }
            
            return RedirectToAction("GetSmsTemplateConfiguration");
        }
    }
}