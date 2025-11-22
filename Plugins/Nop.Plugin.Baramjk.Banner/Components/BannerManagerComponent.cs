using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Baramjk.Banner.Domain;
using Nop.Plugin.Baramjk.Banner.Factories;
using Nop.Plugin.Baramjk.Banner.Models;
using Nop.Plugin.Baramjk.Banner.Plugins;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Mvc.ViewComponents;
using Nop.Plugin.Baramjk.Framework.Services.Ui.DataTables;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Models.DataTables;

namespace Nop.Plugin.Baramjk.Banner.Components
{
    [ViewComponent(Name = "WidgetsBannerManager")]
    public class BannerManagerComponent : BaramjkViewComponent
    {
        private readonly IDataTablesBuilders _dataTablesBuilders;
        private readonly BannerSettings _settings;
        private readonly BannerFactory _bannerFactory;

        public BannerManagerComponent(
            IDataTablesBuilders dataTablesBuilders,
            BannerSettings settings,
            BannerFactory bannerFactory)
        {
            _dataTablesBuilders = dataTablesBuilders;
            _settings = settings;
            _bannerFactory = bannerFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!(await AuthorizeAsync(PermissionProvider.Management)))
                return Content("");

            var id = GetId(additionalData) ?? 0;
            if (additionalData is BaseNopEntityModel entityModel)
                id = entityModel.Id;

            GetZone(widgetZone, out var entityName, out var tags);
            var tablesModel = await _dataTablesBuilders.BuildDataTablesModelAsync(typeof(BannerListItemModel), "Banner",
                readAction: "EntityList");

            tablesModel.Filters = new List<FilterParameter>
            {
                new("EntityId"),
                new("EntityName")
            };

            ViewBag.DataTablesModel = tablesModel;

            ViewBag.Tags = tags;

            return View("Components/WidgetsBannerManager.cshtml",
                new BannerModel
                {
                    Tag = "",
                    BannerType = BannerType.Picture,
                    EntityId = id,
                    EntityName = entityName,
                    AvailableTypes = await _bannerFactory.PrepareBannerTypeSelectListItemsAsync()
                });
        }

        private void GetZone(string widgetZone, out string entityName, out List<string> tags)
        {
            if (widgetZone == AdminWidgetZones.CategoryDetailsBlock)
            {
                entityName = "Category";
                tags = _settings.CategoryTags.SplitSafe();
            }
            else if (widgetZone == AdminWidgetZones.ProductDetailsBlock)
            {
                entityName = "Product";
                tags = _settings.ProductTags.SplitSafe();
            }
            else if (widgetZone == AdminWidgetZones.VendorDetailsBlock)
            {
                entityName = "Vendor";
                tags = _settings.VendorTags.SplitSafe();
            }
            else if (widgetZone == AdminWidgetZones.SpecificationAttributeOptionDetailsBottom)
            {
                entityName = "SpecificationAttributeOption";
                tags = _settings.Tags.SplitSafe();
            }
            else if (widgetZone == AdminWidgetZones.SpecificationAttributeDetailsBlock)
            {
                entityName = "SpecificationAttribute";
                tags = _settings.Tags.SplitSafe();
            }
            else if (widgetZone == AdminWidgetZones.BlogPostDetailsBlock)
            {
                entityName = "Blog";
                tags = _settings.Tags.SplitSafe();
            }
            else if (widgetZone == AdminWidgetZones.TopicDetailsBlock)
            {
                entityName = "Topic";
                tags = _settings.Tags.SplitSafe();
            }
            else if (widgetZone == AdminWidgetZones.NewsItemsDetailsBlock)
            {
                entityName = "News";
                tags = _settings.Tags.SplitSafe();
            }
            else if (widgetZone == AdminWidgetZones.DiscountDetailsBlock)
            {
                entityName = nameof(Discount);
                tags = _settings.Tags.SplitSafe();
            }
            
            else if (widgetZone == AdminWidgetZones.ManufacturerDetailsBlock)
            {
                entityName = "Manufacturer";
                tags = _settings.Tags.SplitSafe();
            }
            else if (widgetZone == AdminWidgetZones.ProductAttributeValueDetailsBottom)
            {
                entityName = "ProductAttributeValue";
                tags = _settings.Tags.SplitSafe();
            }
            else if (widgetZone == AdminWidgetZones.CheckoutAttributeDetailsBlock)
            {
                entityName = "CheckoutAttribute";
                tags = _settings.Tags.SplitSafe();
            }
            else
            {
                tags = new List<string>();
                entityName = null;
            }
        }

        protected override string SystemName => DefaultValue.SystemName;
    }
}