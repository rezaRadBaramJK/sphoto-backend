using System.Threading.Tasks;
using LinqToDB.Common;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.LocationDetector.Models;
using Nop.Plugin.Baramjk.LocationDetector.Plugin;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.LocationDetector.Controllers
{
    [Permission(PermissionProvider.LocationDetectorName)]
    [Area(AreaNames.Admin)]
    public class LocationDetectorConfigurationAdminController : BaseBaramjkPluginAdminController
    {
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly LocationDetectorSettings _locationDetectorSettings;
        private readonly IWorkContext _workContext;
        public LocationDetectorConfigurationAdminController(ISettingService settingService, INotificationService notificationService, LocationDetectorSettings locationDetectorSettings, IWorkContext workContext)
        {
            _settingService = settingService;
            _notificationService = notificationService;
            _locationDetectorSettings = locationDetectorSettings;
            _workContext = workContext;
        }

        public async Task<IActionResult> Configure() => Configure<LocationDetectorSettingsModel>(_locationDetectorSettings);

        [HttpPost]
        public async Task<IActionResult> Configure(LocationDetectorSettings model)
        {
            await _workContext.GetWorkingCurrencyAsync();

            if (!ModelState.IsValid)
                return await Configure();

            await _settingService.SaveSettingModelAsync<LocationDetectorSettings>(model);
            _notificationService.SuccessNotification(await GetResByFullKeyAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        protected override string SystemName => DefaultValue.SystemName;
    }
}