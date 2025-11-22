using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Plugin.Baramjk.PushNotification.Models;
using Nop.Plugin.Baramjk.PushNotification.Plugins;
using Nop.Plugin.Baramjk.PushNotification.Services;
using Nop.Services.Media;

namespace Nop.Plugin.Baramjk.PushNotification.Controllers
{
    public class TokenApiController : BaseBaramjkApiController
    {
        private readonly IPushNotificationTokenService _pushNotificationTokenService;
        private readonly IWorkContext _workContext;

        public TokenApiController(IPushNotificationTokenService pushNotificationTokenService, IWorkContext workContext)
        {
            _pushNotificationTokenService = pushNotificationTokenService;
            _workContext = workContext;
        }

        [HttpPost("/api-frontend/PushNotification/Token")]
        [HttpPost("/FrontendApi/PushNotification/Token")]
        [HttpPost("/PushNotification/Token")]
        public async Task<IActionResult> UpdateToken([FromBody] UpdateTokenModel model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _pushNotificationTokenService.AddOrUpdateAsync(customer.Id, model.Token, model.Platform);
            return ApiResponseFactory.Success();
        }

        [HttpDelete("/api-frontend/PushNotification/Token")]
        [HttpDelete("/FrontendApi/PushNotification/Token")]
        public async Task<IActionResult> DeleteToken([FromBody] UpdateTokenModel model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _pushNotificationTokenService.DeleteAsync(customer.Id, model.Token);
            return ApiResponseFactory.Success();
        }

        [HttpPost("/api-frontend/PushNotification/Token/DeActive")]
        [HttpPost("/FrontendApi/PushNotification/Token/DeActive")]
        public async Task<IActionResult> DeActive()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _pushNotificationTokenService.SetActiveStatusAsync(customer.Id, false);
            return ApiResponseFactory.Success();
        }

        [HttpPost("/api-frontend/PushNotification/Token/Active")]
        [HttpPost("/FrontendApi/PushNotification/Token/Active")]
        public async Task<IActionResult> Active()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _pushNotificationTokenService.SetActiveStatusAsync(customer.Id, true);
            return ApiResponseFactory.Success();
        }
    }
}