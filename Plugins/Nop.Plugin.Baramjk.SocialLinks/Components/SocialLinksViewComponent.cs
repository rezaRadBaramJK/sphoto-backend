using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.ViewComponents;
using Nop.Plugin.Baramjk.SocialLinks.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Baramjk.SocialLinks.Components
{
    [ViewComponent(Name = "WidgetsSocialLinks")]
    public class SocialLinksViewComponent : BaramjkViewComponent
    {
        private readonly SocialLinksSettings _socialLinksSettings;

        public SocialLinksViewComponent(SocialLinksSettings socialLinksSettings)
        {
            _socialLinksSettings = socialLinksSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            switch (widgetZone)
            {
                case "productdetails_bottom":
                case "productdetails_top":
                case "productdetails_overview_top":
                case "productdetails_overview_bottom":
                    return View("ProductSocialLinks.cshtml", _socialLinksSettings);
                default:
                    return View("WidgetsSocialLinks.cshtml", _socialLinksSettings);
            }

            // Console.WriteLine($"widgetZone : {widgetZone}");
            //
            // return View("WidgetsSocialLinks.cshtml", _socialLinksSettings);
        }

        protected override string SystemName => DefaultValue.SystemName;
    }
}