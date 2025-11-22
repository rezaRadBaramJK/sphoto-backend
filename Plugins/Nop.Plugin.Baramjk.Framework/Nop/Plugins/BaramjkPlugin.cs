using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Baramjk.Framework.Nop.Plugins
{
    public abstract class BaramjkPlugin : BasePlugin
    {
        protected readonly IWebHelper WebHelper;
        protected readonly ISettingService SettingService;
        protected readonly IPermissionService PermissionService;
        protected readonly IWorkContext WorkContext;
        protected readonly ICustomerService CustomerService;

        protected BaramjkPlugin()
        {
            WebHelper = EngineContext.Current.Resolve<IWebHelper>();
            SettingService = EngineContext.Current.Resolve<ISettingService>();
            PermissionService = EngineContext.Current.Resolve<IPermissionService>();
            WorkContext = EngineContext.Current.Resolve<IWorkContext>();
            CustomerService = EngineContext.Current.Resolve<ICustomerService>();
        }

        public virtual string SystemName => PluginDescriptor.SystemName;
        private string _name = null;
        public virtual string Name => _name ??= PluginDescriptor.SystemName.Split(".").Last();
        public virtual string FriendlyName => PluginDescriptor.FriendlyName;
        public virtual string PluginMenuSystemName => MenuUtils.BuildPluginMenuSystemName(SystemName);
        public virtual bool AutoAddConfigurationMenu => true;

        public virtual string MenuSystemNameConfiguration =>
            MenuUtils.BuildPluginMenuSystemName(SystemName, "Configuration");

        public override string GetConfigurationPageUrl()
        {
            return $"{WebHelper.GetStoreLocation()}Admin/{Name}Admin/Configure";
        }

        protected SiteMapNode CreatePluginSiteMapNode(string title = null, params SiteMapNode[] nodes)
        {
            var pluginMenu = new SiteMapNode
            {
                SystemName = PluginMenuSystemName,
                Title = title ?? FriendlyName,
                IconClass = MenuUtils.IconClassPlugin,
                Visible = true,
                RouteValues = new RouteValueDictionary { { "Area", "Admin" } }
            };

            if (nodes.HasItem())
                foreach (var node in nodes)
                    pluginMenu.ChildNodes.Add(node);

            if (AutoAddConfigurationMenu)
            {
                var configurationPageUrl = GetConfigurationPageUrl();
                if (configurationPageUrl.HasValue())
                    pluginMenu.ChildNodes.Add(CreateConfigurationSiteMapNode(configurationPageUrl));
            }

            return pluginMenu;
        }

        protected virtual SiteMapNode CreateConfigurationSiteMapNode(string url)
        {
            var node = new SiteMapNode
            {
                SystemName = MenuSystemNameConfiguration,
                Title = "Configuration",
                IconClass = "fas fa-cog",
                Url = url,
                Visible = true,
            };
            return node;
        }

        protected SiteMapNode CreateSiteMapNode(string controllerName, string actionName, string title = null
            , string systemName = null, string iconClass = MenuUtils.IconClassItem)
        {
            var node = new SiteMapNode
            {
                Title = title ?? actionName,
                ControllerName = controllerName,
                ActionName = actionName,
                IconClass = iconClass,
                Visible = true,
                RouteValues = new RouteValueDictionary { { "Area", "Admin" } },
                SystemName = systemName ?? MenuUtils.BuildPluginMenuSystemName(SystemName, controllerName, actionName),
            };

            return node;
        }

        public virtual async Task<bool> IsAdminAsync() =>
            await CustomerService.IsAdminAsync(await WorkContext.GetCurrentCustomerAsync());

        public virtual async Task<bool> AuthorizeAsync(params PermissionRecord[] permissions)
        {
            var customer = await WorkContext.GetCurrentCustomerAsync();
            return await permissions.AnyAwaitAsync(async permission =>
                await PermissionService.AuthorizeAsync(permission, customer));
        }
    }
}