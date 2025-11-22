using Microsoft.AspNetCore.Mvc.Controllers;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Models.ViewModels;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Web.Framework.Mvc.Razor;
using Nop.Web.Framework.UI;

namespace Nop.Plugin.Baramjk.Framework.Mvc.Razor
{
    public abstract class BaramjkRazorPage<TModel> : NopRazorPage<TModel>
    {
        protected BaramjkRazorPage()
        {
        }

        protected virtual ControllerActionDescriptor ActionDescriptor =>
            ViewContext?.ActionDescriptor as ControllerActionDescriptor;

        protected virtual string SystemName =>
            ActionDescriptor.ControllerTypeInfo.Assembly.GetName().Name?.Replace("Nop.Plugin.", "");

        protected virtual string ControllerName => ActionDescriptor?.ControllerName;
        protected virtual string ActionName => ActionDescriptor?.ActionName;

        public static void SetActiveMenuByMenuSystemName(string activeMenuSystemName)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.SetActiveMenuItemSystemName(activeMenuSystemName);
        }

        public static void SetActiveMenu(string systemName, string controllerName, string action)
        {
            var activeMenuSystemName = MenuUtils.BuildPluginMenuSystemName(systemName, controllerName, action);
            SetActiveMenuByMenuSystemName(activeMenuSystemName);
        }

        public void SetActiveMenu(string action)
        {
            var activeMenuSystemName = MenuUtils.BuildPluginMenuSystemName(SystemName, ControllerName, action);
            SetActiveMenuByMenuSystemName(activeMenuSystemName);
        }

        public void SetActiveMenu()
        {
            var activeMenuSystemName = MenuUtils.BuildPluginMenuSystemName(SystemName, ControllerName, ActionName);
            SetActiveMenuByMenuSystemName(activeMenuSystemName);
        }

        public void InitConfigureView(DefaultConfigureViewModel model)
        {
            Layout = FrameworkDefaultValues.ConfigureLayoutViewPath;
            ViewBag.ConfigureModel = model;
        }
    }
}