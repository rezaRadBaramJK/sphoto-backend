using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Nop.Plugins;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Baramjk.SocialLinks.Plugins
{
    public class SocialLinksPlugin : BaramjkPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        private readonly ILocalizationService _localizationService;

        public static SocialLinksSettings GetDefaultSetting => new SocialLinksSettings
        {
            WidgetIconSize = "50px",
            PaymentMethodIconSize = "50px",
            SocialMediaIconSize = "50px",
            // Icon1 = "/Plugins/Baramjk.SocialLinks/Icons/WhatsApp.png",
            // Link1 = "https://api.whatsapp.com/send?phone=+965000000&text=ALSALAM ALAYKOM , السلام عليكم",
            //
            // Icon2 = "/Plugins/Baramjk.SocialLinks/Icons/instagram.png",
            // Link2 = "https://www.instagram.com/XYZ",
        };

        public static Dictionary<string, string> GetLocalizationResources => new()
        {
            { "Admin.Plugins.SocialLinks.SocialLink.Name", "Name" },
            { "Admin.Plugins.SocialLinks.SocialLink.Link", "Link" },
            { "Admin.Plugins.SocialLinks.SocialLink.Priority", "Priority" },
            { "Admin.Plugins.SocialLinks.SocialLink.Image", "Image" },
            { "Admin.Plugins.SocialLinks.SocialLink.ImageId", "Image" },
            { "Admin.Plugins.SocialLinks.SocialLink.ShowInFooter", "Show In Footer" },
            { "Admin.Plugins.SocialLinks.SocialLink.ShowInWidget", "Show In Widget" },
            { "Admin.Plugins.SocialLinks.SocialLink.ShowInProductDetails", "Show In Product Details" },
            { "Admin.Plugins.SocialLinks.SocialLink.SocialSharePrefix", "Social Share Prefix" },
            { "Admin.Plugin.SocialLinks.SocialLinks.Title", "Social links" },
        };
        public override async Task InstallAsync()
        {
            await SettingService.SaveSettingAsync(GetDefaultSetting);
            await PermissionService.InstallPermissionsAsync(new PermissionProvider());
            await _localizationService.AddLocaleResourceAsync(GetLocalizationResources);
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await SettingService.DeleteSettingAsync<SocialLinksSettings>();
            await base.UninstallAsync();
        }

        protected static SiteMapNode _pluginSiteMapNode;

        public SocialLinksPlugin(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (await AuthorizeAsync(PermissionProvider.SocialLinksManagement) == false)
                return;

            if (_pluginSiteMapNode == null)
            {
                // var createSocialLink = CreateSiteMapNode("SocialLinksEntity", "CreateSocialMedia",
                //     "Create SocialMedia",
                //     $"{MenuUtils.BaramjkMenuSystemName}_SocialLinks_SocialLinks_CreateSocialMedia", "fa-subscription-management");
                
                var listSocialLink = CreateSiteMapNode("SocialLinksEntity", "ListSocialMedia",
                    "List SocialMedia",
                    $"{MenuUtils.BaramjkMenuSystemName}_SocialLinks_SocialLinks_ListSocialMedia", "fa-subscription-management");
                
                // var createPaymentMethod = CreateSiteMapNode("SocialLinksEntity", "CreatePaymentMethod",
                //     "Create PaymentMethod",
                //     $"{MenuUtils.BaramjkMenuSystemName}_SocialLinks_SocialLinks_CreatePaymentMethod", "fa-subscription-management");
                
                var listPaymentLink = CreateSiteMapNode("SocialLinksEntity", "ListPaymentMethod",
                    "List PaymentMethod",
                    $"{MenuUtils.BaramjkMenuSystemName}_SocialLinks_SocialLinks_ListPaymentMethod", "fa-subscription-management");
                _pluginSiteMapNode = CreatePluginSiteMapNode("Social Link",listSocialLink,listPaymentLink);
                _pluginSiteMapNode.SystemName = $"{MenuUtils.BaramjkMenuSystemName}_SocialLinks";
            }
            // _pluginSiteMapNode = CreatePluginSiteMapNode();

            MenuUtils.AddToBaramjkPluginsMenu(rootNode, _pluginSiteMapNode);
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "WidgetsSocialLinks";
        }

        public bool HideInWidgetList => false;

        public async Task<IList<string>> GetWidgetZonesAsync()
        {
            return new List<string>
            {
                // PublicWidgetZones.HomepageBottom,
                // PublicWidgetZones.ProductDetailsBottom,
                // PublicWidgetZones.ProductDetailsTop,
                // PublicWidgetZones.ProductDetailsOverviewTop,
                // PublicWidgetZones.ProductDetailsOverviewBottom
            };
        }
    }
}