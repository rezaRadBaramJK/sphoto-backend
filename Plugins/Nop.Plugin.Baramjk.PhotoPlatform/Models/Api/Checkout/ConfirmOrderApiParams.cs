namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Checkout
{
    public class ConfirmOrderApiParams
    {
        public bool ProcessPayment { get; set; } = true;
        public string Device { get; set; } = "Web";
        public string PaymentMethod { get; set; } = "Cash";
    }
}