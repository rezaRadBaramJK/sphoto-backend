using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Localization;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.UI;

namespace Nop.Plugin.Baramjk.Framework.Mvc.Controller
{
    public abstract class BaseBaramjkPluginController : BasePluginController
    {
        protected readonly ILocalizationService _localizationService;

        protected BaseBaramjkPluginController()
        {
            _localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        }

        protected virtual string SystemName => GetType().Assembly.GetName().Name?.Replace("Nop.Plugin.", "");
        protected virtual string ControllerName => GetType().Name.Replace("Controller", "");
        protected virtual string FolderName => $"{ControllerName}s";

        public void SetActiveMenuByMenuSystemName(string activeMenuSystemName)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.SetActiveMenuItemSystemName(activeMenuSystemName);
            ViewBag.IsSetActiveMenu = true;
        }

        public void SetActiveMenu(string systemName, string controllerName, string action)
        {
            var activeMenuSystemName = MenuUtils.BuildPluginMenuSystemName(systemName, controllerName, action);
            SetActiveMenuByMenuSystemName(activeMenuSystemName);
        }

        public void SetActiveMenu(string action)
        {
            var activeMenuSystemName = MenuUtils.BuildPluginMenuSystemName(SystemName, ControllerName, action);
            SetActiveMenuByMenuSystemName(activeMenuSystemName);
        }

        protected async Task<string> GetResByFullKeyAsync(string key) =>
            await _localizationService.GetResourceAsync(key);

        protected async Task<string> GetResAsync(string key) =>
            await _localizationService.GetResourceAsync($"{SystemName}.{key}");

        protected new virtual ViewResult View(string viewName)
        {
            return ((Microsoft.AspNetCore.Mvc.Controller)this).View(GetViewPath(viewName));
        }

        protected new virtual ViewResult View(string viewName, object model)
        {
            return ((Microsoft.AspNetCore.Mvc.Controller)this).View(GetViewPath(viewName), model);
        }

        protected virtual ViewResult ViewBase(string viewName)
        {
            return ((Microsoft.AspNetCore.Mvc.Controller)this).View(viewName);
        }

        protected virtual ViewResult ViewBase(string viewName, object model)
        {
            return ((Microsoft.AspNetCore.Mvc.Controller)this).View(viewName, model);
        }

        protected virtual string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Views/{viewName}";
        }
    }
}