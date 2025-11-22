using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Baramjk.Framework.Mvc.ViewComponents;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Banner.Components
{
    [ViewComponent(Name = "WidgetsBanner")]
    public class BannerViewComponent : BaramjkViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var id = 0;
            if (additionalData is BaseNopEntityModel model)
                id = model.Id;
            
            if (id == 0)
                return Content("");

            if (widgetZone == AdminWidgetZones.CategoryDetailsBlock)
                return CategoryDetailsBlock(id);

            if (widgetZone == AdminWidgetZones.ProductDetailsBlock)
                return ProductDetailsBlock(id);

            if (widgetZone == AdminWidgetZones.VendorDetailsBlock)
                return VendorDetailsBlock(id);

            if (widgetZone == AdminWidgetZones.SpecificationAttributeOptionDetailsBottom)
                return EntityDetailsBlock(id, "SpecificationAttributeOption");

            if (widgetZone == AdminWidgetZones.SpecificationAttributeDetailsBlock)
                return EntityDetailsBlock(id, "SpecificationAttribute");

            if (widgetZone == AdminWidgetZones.BlogPostDetailsBlock)
                return EntityDetailsBlock(id, "Blog");

            if (widgetZone == AdminWidgetZones.TopicDetailsBlock)
                return EntityDetailsBlock(id, "Topic");

            if (widgetZone == AdminWidgetZones.NewsItemsDetailsBlock)
                return EntityDetailsBlock(id, "News");
            
            if (widgetZone == AdminWidgetZones.DiscountDetailsBlock)
                return EntityDetailsBlock(id, nameof(Discount));
            
            
            if (widgetZone == AdminWidgetZones.ManufacturerDetailsBlock)
                return ManufacturerDetailsBlock(id, "Manufacturer");
            if (widgetZone == AdminWidgetZones.ProductAttributeValueDetailsBottom)
                return ManufacturerDetailsBlock(id, "ProductAttributeValue");
            if (widgetZone == AdminWidgetZones.CheckoutAttributeDetailsBlock)
                return EntityDetailsBlock(id, "CheckoutAttribute");

            return Content("");
        }

        private IViewComponentResult CategoryDetailsBlock(int id)
        {
            ViewBag.Id = id;
            return View("Components/CategoryDetailsBlock.cshtml");
        }

        private IViewComponentResult ProductDetailsBlock(int id)
        {
            ViewBag.Id = id;
            return View("Components/ProductDetailsBlock.cshtml");
        }

        private IViewComponentResult VendorDetailsBlock(int id)
        {
            ViewBag.Id = id;
            return View("Components/VendorDetailsBlock.cshtml");
        }
        
        
        private IViewComponentResult ManufacturerDetailsBlock(int id, string entityType)
        {
            ViewBag.Id = id;
            ViewBag.entityType = entityType;
            return View("Components/EntityDetailsBlock.cshtml");
        }

        
        private IViewComponentResult EntityDetailsBlock(int id, string entityType)
        {
            ViewBag.Id = id;
            ViewBag.entityType = entityType;
            return View("Components/EntityDetailsBlock.cshtml");
        }

        protected override string SystemName => DefaultValue.SystemName;
    }
}