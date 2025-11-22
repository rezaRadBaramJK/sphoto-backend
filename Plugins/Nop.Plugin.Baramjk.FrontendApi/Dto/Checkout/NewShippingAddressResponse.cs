namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class NewShippingAddressResponse : CheckoutRedirectResponse
    {
        public CheckoutShippingAddressModelDto Model { get; set; }
    }
}