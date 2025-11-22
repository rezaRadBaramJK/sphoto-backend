namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Checkout
{
    public class ConfirmOrderResponse : CheckoutRedirectResponse
    {
        public CheckoutConfirmModelDto Model { get; set; }

        public string Url { get; set; }
    }
}