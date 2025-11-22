using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Security;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PushNotification.Models.WhatsApp;
using Nop.Plugin.Baramjk.PushNotification.Plugins;
using Nop.Services.Configuration;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.PushNotification.Controllers
{
    [Area(AreaNames.Admin)]
    public class WhatsAppAdminController 
        : BaseBaramjkPluginAdminController<WhatsAppSettings, WhatsAppSettingViewModel>
    {

        private readonly ISettingService _settingService;
        
        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Views/Menu/WhatsApp/{viewName}";
        }
        

        public WhatsAppAdminController(ISettingService settingService)
        {
            _settingService = settingService;
        }


        protected override PermissionRecord ConfigurePermissionRecord => PermissionProvider.Management;
        

        
        [HttpPost]
        public async Task<IActionResult> UpdateWhatsAppConfigurationAsync(WhatsAppSettingViewModel viewModel)
        {
            await _settingService.SaveSettingModelAsync<WhatsAppSettings>(viewModel);
            return ApiResponseFactory.Success();
        }
        
    }
}