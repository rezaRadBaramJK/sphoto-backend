using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.ViewComponents;

namespace Nop.Plugin.Baramjk.Wallet.Components
{
    [ViewComponent(Name = "PaymentInfo")]
    public class PaymentInfoViewComponent : BaramjkViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("PaymentInfoViewComponent/PaymentInfo.cshtml");
        }

        protected override string SystemName => DefaultValue.SystemName;
    }
}