namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class ConfirmOrderResponse : CheckoutRedirectResponse
    {
        public CheckoutConfirmModelDto Model { get; set; }
    }
}