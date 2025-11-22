using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Nop.Plugins;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Baramjk.Banner.Plugins
{
    public class BannerPlugin : BaramjkPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        public static BannerSettings GetDefaultSetting => new()
        {
            Tags = "HomePageSlider",
            ProductTags = "Slider",
            CategoryTags = "Banner,SvgIcon",
            VendorTags = "Banner",
            ShowInProductAttributeValuePage = false

        };

        public static Dictionary<string, string> GetLocalizationResources => new()
        {
                {"Nop.Plugin.Baramjk.Banner.Admin.ShowInProductAttributeValuePage", "Show In Product Attribute Value Page"},
                {"Nop.Plugin.Baramjk.Banner.Admin.Banner.ExpirationDateTime", "Expiration Date"},

                // General
                {"Admin.Plugins.Banner.List", "Banner List"},
                {"Admin.Plugins.Banner.Management", "Banner Management"},

                // Fields
                {"Admin.Plugins.Banner.Fields.Title", "Title"},
                {"Admin.Plugins.Banner.Fields.Text", "Text"},
                {"Admin.Plugins.Banner.Fields.FileName", "File Name"},
                {"Admin.Plugins.Banner.Fields.Link", "Link"},
                {"Admin.Plugins.Banner.Fields.EntityName", "Entity Name"},
                {"Admin.Plugins.Banner.Fields.EntityId", "Entity ID"},
                {"Admin.Plugins.Banner.Fields.ExpirationDateTime", "Expiration Date Time"},
                {"Admin.Plugins.Banner.Fields.Type", "Type"},
                {"Admin.Plugins.Banner.Fields.Picture", "Picture"},
                {"Admin.Plugins.Banner.Fields.Tag", "Tags"},
                {"Admin.Plugins.Banner.Fields.AltText", "Alt Text"},
                {"Admin.Plugins.Banner.Fields.DisplayOrder", "Display Order"},

                // Search
                {"Admin.Plugins.Banner.Search.Title", "Search Title"},
                {"Admin.Plugins.Banner.Search.Tag", "Search Tag"},
                {"Admin.Plugins.Banner.Search.EntityName", "Search Entity Name"},
                {"Admin.Plugins.Banner.Search.EntityId", "Search Entity ID"},
                {"Admin.Plugins.Banner.Search.ExpirationDateFrom", "Expiration Date From"},
                {"Admin.Plugins.Banner.Search.ExpirationDateTo", "Expiration Date To"},

                // Messages
                {"Admin.Plugins.Banner.Added", "The banner has been added successfully"},
                {"Admin.Plugins.Banner.Edited", "The banner has been updated successfully"},
                {"Admin.Plugins.Banner.Deleted", "The banner has been deleted successfully"},

                // Validation
                {"Admin.Plugins.Banner.Fields.Title.Required", "Title is required"},
                {"Admin.Plugins.Banner.Fields.EntityName.Required", "Entity Name is required"},
                {"Admin.Plugins.Banner.Fields.EntityId.Required", "Entity ID is required"},

                // Buttons
                {"Admin.Plugins.Banner.AddNew", "Add new banner"},
                {"Admin.Plugins.Banner.Edit", "Edit banner"},
                {"Admin.Plugins.Banner.BackToList", "Back to banner list"}
        };

        private readonly ILocalizationService _localizationService;

        public BannerPlugin(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public override async Task InstallAsync()
        {
            await SettingService.SaveSettingAsync(GetDefaultSetting);
            await _localizationService.AddLocaleResourceAsync(GetLocalizationResources);
            await PermissionService.InstallPermissionsAsync(new PermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await SettingService.DeleteSettingAsync<BannerSettings>();
            await _localizationService.DeleteLocaleResourcesAsync("Nop.Plugin.Baramjk.Banner");
            await base.UninstallAsync();
        }

        private static SiteMapNode _cachePluginSiteMapNode;

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (!(await AuthorizeAsync(PermissionProvider.Management)))
                return;

            if (_cachePluginSiteMapNode == null)
            {
                var node = CreateSiteMapNode(
                    "Banner",
                    "List",
                    "List",
                    $"Baramjk.Banner"
                    );
                _cachePluginSiteMapNode = CreatePluginSiteMapNode(FriendlyName, nodes: node);
            }

            rootNode.AddToBaramjkPluginsMenu(_cachePluginSiteMapNode);
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "WidgetsBannerManager";
        }

        public bool HideInWidgetList => false;

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                AdminWidgetZones.CategoryDetailsBlock,
                AdminWidgetZones.ProductDetailsBlock,
                AdminWidgetZones.VendorDetailsBlock,
                AdminWidgetZones.SpecificationAttributeOptionDetailsBottom,
                AdminWidgetZones.SpecificationAttributeDetailsBlock,
                AdminWidgetZones.NewsItemsDetailsBlock,
                AdminWidgetZones.BlogPostDetailsBlock,
                AdminWidgetZones.TopicDetailsBlock,
                AdminWidgetZones.ManufacturerDetailsBlock,
                AdminWidgetZones.ProductAttributeValueDetailsBottom,
                AdminWidgetZones.DiscountDetailsBlock,
                AdminWidgetZones.CheckoutAttributeDetailsBlock    
            });
        }
    }
}
