using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Plugin.Baramjk.OrderPaymentLink.Models;
using Nop.Plugin.Baramjk.OrderPaymentLink.Services.Model;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.OrderPaymentLink.Controllers.Admin
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class OrderPaymentPluginController : BaseBaramjkPluginController
    {
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;

        public OrderPaymentPluginController(ISettingService settingService, INotificationService notificationService)
        {
            _settingService = settingService;
            _notificationService = notificationService;
        }

        [HttpGet]
        [Route("/Admin/OrderPaymentLinkAdmin/Configure")]
        public async Task<IActionResult> Configure()
        {
            var settings = await _settingService.LoadSettingAsync<OrderPaymentLinkSetting>();
            var model = MapUtils.Map<OrderPaymentLinkSettingsModel>(settings);
            return View("/Configure.cshtml", model);
        }

        [HttpPost]
        [Route("/Admin/OrderPaymentLinkAdmin/Configure")]
        public async Task<IActionResult> Configure(OrderPaymentLinkSettingsModel model)
        {
            var settings = MapUtils.Map<OrderPaymentLinkSetting>(model);
            await _settingService.SaveSettingAsync(settings);
            _notificationService.SuccessNotification("settings saved successfully.");
            return await Configure();
        }
    }
}