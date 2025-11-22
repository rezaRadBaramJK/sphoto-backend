using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class CheckoutPaymentInfoModelDto : ModelDto
    {
        public string PaymentViewComponentName { get; set; }

        /// <summary>
        ///     Used on one-page checkout page
        /// </summary>
        public bool DisplayOrderTotals { get; set; }
    }
}