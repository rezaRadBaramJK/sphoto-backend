using System;

namespace Nop.Plugin.Baramjk.Framework.Models.ViewModels
{
    public class DefaultConfigureViewModel
    {
        public DefaultConfigureViewModel(Type pluginType, string controller)
        {
            PluginType = pluginType;
            Controller = controller;
        }

        public DefaultConfigureViewModel(string friendlyName, string activeMenuSystemName, string controller)
        {
            FriendlyName = friendlyName;
            ActiveMenuSystemName = activeMenuSystemName;
            Controller = controller;
        }

        public Type PluginType { get; set; }
        public string FriendlyName { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; } = "Configure";
        public string ActiveMenuSystemName { get; set; } = "Widgets";
        public string ConfigureLocale { get; set; } = "Admin.ContentManagement.Widgets.Configure";
        public string BackToLocale { get; set; } = "Admin.ContentManagement.Widgets.BackToList";
        public string ListActionMethodName { get; set; } = "List";
        public string ListControllerName { get; set; } = "Widget";
    }
}