using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Plugin.Baramjk.OtpAuthentication.Factories;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Settings;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Types;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Controller
{
    [Area(AreaNames.Admin)]
    public class OtpAuthenticateController  : BaseBaramjkPluginAdminController
    {
        private readonly OtpAuthenticationSettings _otpAuthenticationSettings;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly SettingFactory _settingFactory;

        public OtpAuthenticateController(OtpAuthenticationSettings otpAuthenticationSettings, ISettingService settingService,
            INotificationService notificationService, SettingFactory settingFactory)
        {
            _otpAuthenticationSettings = otpAuthenticationSettings;
            _settingService = settingService;
            _notificationService = notificationService;
            _settingFactory = settingFactory;
        }

        [HttpGet("Admin/OtpAuthenticationAdmin/Configure")]
        public async Task<IActionResult> Configure()
        {
            var model = await _settingFactory.PrepareViewModelAsync(_otpAuthenticationSettings);
            return View("Configure.cshtml", model);
        }

        [HttpPost("Admin/OtpAuthenticationAdmin/Configure")]
        public async Task<IActionResult> Configure(OtpAuthenticationSettingsModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            var settings = MapUtils.Map<OtpAuthenticationSettings>(model);
            settings.SendMethod = (SendMethodType) model.SendMethodId;
            await _settingService.SaveSettingAsync(settings);
            _notificationService.SuccessNotification(await GetResByFullKeyAsync("Admin.Plugins.Saved"));

            return await Configure();
        }
        
        protected override string SystemName => "Baramjk.OtpAuthentication";
    }
}
