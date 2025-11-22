namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class NewBillingAddressResponse : CheckoutRedirectResponse
    {
        public CheckoutBillingAddressModelDto Model { get; set; }
    }
}