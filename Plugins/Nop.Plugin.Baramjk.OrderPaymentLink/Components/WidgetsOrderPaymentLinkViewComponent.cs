using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Baramjk.Framework.Mvc.ViewComponents;
using Nop.Plugin.Baramjk.OrderPaymentLink.ImplementNopPlugin;
using Nop.Plugin.Baramjk.OrderPaymentLink.Services;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Models.Checkout;

namespace Nop.Plugin.Baramjk.OrderPaymentLink.Components
{
    [ViewComponent(Name = "WidgetsOrderPaymentLink")]
    public class WidgetsOrderPaymentLinkViewComponent : BaramjkViewComponent
    {
        private readonly InvoiceService _invoiceService;

        public WidgetsOrderPaymentLinkViewComponent(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (widgetZone == AdminWidgetZones.OrderDetailsBlock)
                return await OrderDetailsBlockAsync(additionalData);

            if (widgetZone == PublicWidgetZones.CheckoutCompletedBottom)
                return await CheckoutCompletedBottomAsync(additionalData);

            return Content("");
        }
        
        private async Task<IViewComponentResult> OrderDetailsBlockAsync(object additionalData)
        {
            if (!await _permissionService.AuthorizeAsync(PermissionRecords.CreateOrderPaymentLink))
                return Content("");

            var orderModel = (OrderModel)additionalData;
            ViewBag.Id = orderModel.Id;
            var translations = await _invoiceService.GetEntityTranslationsAsync(orderModel.Id);

            return View("WidgetsOrderPaymentLink/OrderDetailsBlock.cshtml", translations);
        }

        private async Task<IViewComponentResult> CheckoutCompletedBottomAsync(object additionalData)
        {
            var orderModel = (CheckoutCompletedModel) additionalData;
            var orderId = orderModel.OrderId;

            if (await _invoiceService.GetOrderPaymentStatus(orderId) != PaymentStatus.Pending)
                return Content("");

            ViewBag.Id = orderId;
            return View("WidgetsOrderPaymentLink/CheckoutCompletedBottom.cshtml");
        }
    }
}