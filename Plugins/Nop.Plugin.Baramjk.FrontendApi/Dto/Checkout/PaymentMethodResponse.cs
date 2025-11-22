namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class PaymentMethodResponse : CheckoutRedirectResponse
    {
        public CheckoutPaymentMethodModelDto Model { get; set; }
    }
}