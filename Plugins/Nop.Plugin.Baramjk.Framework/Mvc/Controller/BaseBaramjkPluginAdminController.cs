using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using Nop.Services.Security;

#pragma warning disable CS1998

namespace Nop.Plugin.Baramjk.Framework.Mvc.Controller
{
    public abstract class BaseBaramjkPluginAdminController : BaseBaramjkPluginController
    {
        protected virtual ViewResult Configure<TSettingModel>(object settings)
        {
            var model = MapUtils.Map<TSettingModel>(settings);
            return View("Configure.cshtml", model);
        }
    }

    public abstract class BaseBaramjkPluginAdminController<TSetting, TSettingModel>
        : BaseBaramjkPluginAdminController where TSetting : ISettings, new()
    {
        private readonly TSetting _settings;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        protected readonly IPermissionService _permissionService;
        protected readonly IWorkContext _workContext;

        protected BaseBaramjkPluginAdminController()
        {
            _settings = (TSetting)EngineContext.Current.Resolve(typeof(TSetting));
            _notificationService = (INotificationService)EngineContext.Current.Resolve(typeof(INotificationService));
            _settingService = (ISettingService)EngineContext.Current.Resolve(typeof(ISettingService));
            _permissionService = EngineContext.Current.Resolve<IPermissionService>();
            _workContext = EngineContext.Current.Resolve<IWorkContext>();
        }

        protected abstract PermissionRecord ConfigurePermissionRecord { get; }

        public virtual async Task<IActionResult> Configure()
        {
            if (ConfigurePermissionRecord != null &&
                !await _permissionService.AuthorizeAsync(ConfigurePermissionRecord))
                return AccessDeniedView();

            return Configure<TSettingModel>(_settings);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Configure(TSettingModel model)
        {
            if (ConfigurePermissionRecord != null &&
                !await _permissionService.AuthorizeAsync(ConfigurePermissionRecord))
                return AccessDeniedView();

            if (!ValidateConfigureModel(model))
                return await Configure();

            await _settingService.SaveSettingModelAsync<TSetting>(model);
            _notificationService.SuccessNotification(await GetResByFullKeyAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        protected virtual bool ValidateConfigureModel(TSettingModel model)
        {
            return ModelState.IsValid;
        }
    }
}