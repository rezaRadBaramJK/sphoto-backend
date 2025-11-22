using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.UI;

namespace Nop.Plugin.Baramjk.Framework.Utils
{
    public static class MenuUtils
    {
        public const string BaramjkMenuSystemName = "BaramjkMenu";
        public static string BaramjkPluginsMenuSystemName = $"{BaramjkMenuSystemName}_Plugins";
        public const string IconClassPlugin = "far fa fa-puzzle-piece";
        public const string IconClassItem = "far fa-dot-circle";
        public const string IconClassSubsItem = "far fa-circle";

        public static SiteMapNode GetBaramjkMenu(SiteMapNode rootNode)
        {
            var baramjkNop = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == BaramjkMenuSystemName);
            if (baramjkNop != null)
                return baramjkNop;

            baramjkNop = new SiteMapNode
            {
                SystemName = BaramjkMenuSystemName,
                Title = "Baramjk",
                IconClass = " fas fa-building",
                Visible = true
            };

           /* var license = new SiteMapNode
            {
                SystemName = $"{BaramjkMenuSystemName}_License",
                Title = "License",
                ControllerName = "LicensePanel",
                ActionName = "List",
                IconClass = "fas far fa fa-award",
                Visible = true,
                RouteValues = new RouteValueDictionary { { "Area", "Admin" } }
            };
*/
            var plugin = new SiteMapNode
            {
                SystemName = $"{BaramjkMenuSystemName}_Plugins",
                Title = "Plugins",
                IconClass = "fas  fa-cubes",
                Visible = true
            };

            var contactus = new SiteMapNode
            {
                SystemName = $"{BaramjkMenuSystemName}_ContactUs",
                Title = "Contact Us",
                IconClass = "far fa fa-comment",
                Visible = true,
                Url = "https://baramjk.com/contact.html",
                OpenUrlInNewTab = true
            };

            var ticket = new SiteMapNode
            {
                SystemName = $"{BaramjkMenuSystemName}_Ticket",
                Title = "Send Ticket",
                IconClass = "far fa fa-comments",
                Visible = true,
                Url = "https://baramjk.baramjk.com/customer/addticket",
                OpenUrlInNewTab = true
            };

            var help = new SiteMapNode
            {
                SystemName = $"{BaramjkMenuSystemName}_Help",
                Title = "Help",
                IconClass = "fas fa-question-circle",
                Visible = true,
                Url = "https://baramjk.com/",
                OpenUrlInNewTab = true
            };

            var pluginInformation = new SiteMapNode
            {
                SystemName = $"{BaramjkMenuSystemName}_Plugins_Information",
                Title = "Plugin information",
                IconClass = "fas far fa fa-info",
                ControllerName = "BaramjkPlugin",
                ActionName = "List",
                RouteValues = new RouteValueDictionary { { "Area", "Admin" } },
                Visible = true
            };

           // baramjkNop.ChildNodes.Add(license);
            baramjkNop.ChildNodes.Add(plugin);
            baramjkNop.ChildNodes.Add(pluginInformation);
            baramjkNop.ChildNodes.Add(contactus);
            baramjkNop.ChildNodes.Add(ticket);
            baramjkNop.ChildNodes.Add(help);

            rootNode.ChildNodes.Add(baramjkNop);

            return baramjkNop;
        }

        public static SiteMapNode GetBaramjkPluginsMenu(SiteMapNode rootNode)
        {
            var baramjkNop = GetBaramjkMenu(rootNode).ChildNodes
                .FirstOrDefault(x => x.SystemName == BaramjkPluginsMenuSystemName);
            return baramjkNop;
        }

        public static void AddToBaramjkMenu(SiteMapNode rootNode, SiteMapNode newMenu)
        {
            var baramjkNop = GetBaramjkMenu(rootNode);
            baramjkNop.ChildNodes.Add(newMenu);
        }

        public static void AddToBaramjkPluginsMenu(this SiteMapNode rootNode, SiteMapNode newMenu)
        {
            var baramjkNop = GetBaramjkPluginsMenu(rootNode);
            baramjkNop.ChildNodes.Add(newMenu);
        }

        [Obsolete]
        public static void AddMenuToBaramjkMenu(SiteMapNode rootNode, SiteMapNode newMenu)
        {
            AddToBaramjkPluginsMenu(rootNode, newMenu);
        }

        public static string BuildPluginMenuSystemName(string systemName) => $"{BaramjkMenuSystemName}_{systemName}";

        public static string BuildPluginMenuSystemName(string systemName, string section) =>
            $"{BaramjkMenuSystemName}_{systemName}_{section}";

        public static string BuildPluginMenuSystemName(string systemName, string section, string section2) =>
            $"{BaramjkMenuSystemName}_{systemName}_{section}_{section2}";

        public static void SetActiveMenu(this IHtmlHelper html, string systemName, string controllerName,
            string actionName)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            var systemNameMenu = BuildPluginMenuSystemName(systemName, controllerName, actionName);
            pageHeadBuilder.SetActiveMenuItemSystemName(systemNameMenu);
        }

        public static void SetActiveMenu(this IHtmlHelper html, DefaultListViewModel model)
        {
            if (model.ActiveMenuSystemName == "")
                return;

            model.ActiveMenuSystemName ??=
                BuildPluginMenuSystemName(model.SystemName, model.ControllerName, model.ReadAction);

            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.SetActiveMenuItemSystemName(model.ActiveMenuSystemName);
        }
    }
}