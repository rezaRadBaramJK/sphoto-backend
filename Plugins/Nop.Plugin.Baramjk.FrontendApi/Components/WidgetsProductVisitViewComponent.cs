using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Services.Products;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Baramjk.FrontendApi.Components
{
    [ViewComponent(Name = "WidgetsProductVisit")]
    public class WidgetsProductVisitViewComponent : NopViewComponent
    {
        private readonly IProductVisitService _productVisitService;

        public WidgetsProductVisitViewComponent(IProductVisitService productVisitService)
        {
            _productVisitService = productVisitService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (widgetZone == AdminWidgetZones.ProductDetailsBlock)
                return AdminUpdateVisit(additionalData);

            if (widgetZone == PublicWidgetZones.ProductDetailsBottom)
                return VisitCounter(additionalData);

            return Content("");
        }

        private IViewComponentResult AdminUpdateVisit(object additionalData)
        {
            var productModel = additionalData as ProductModel;
            var visit = _productVisitService.GetVisitAsync(productModel.Id).Result;
            ViewBag.Id = productModel.Id;
            ViewBag.Count = visit;

            return View(
                "~/Plugins/Baramjk.FrontendApi/Views/WidgetsProductVisitViewComponent.cshtml");
        }

        private IViewComponentResult VisitCounter(object additionalData)
        {
            var productModel = additionalData as ProductDetailsModel;
            _productVisitService.IncreaseVisitAsync(productModel.Id).Wait();
            return Content("");
        }
    }
}