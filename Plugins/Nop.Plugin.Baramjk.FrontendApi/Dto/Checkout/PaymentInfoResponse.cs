using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class PaymentInfoResponse : BaseDto
    {
        public CheckoutConfirmModelDto CheckoutConfirmModel { get; set; }

        public CheckoutPaymentInfoModelDto CheckoutPaymentInfoModel { get; set; }
    }
}