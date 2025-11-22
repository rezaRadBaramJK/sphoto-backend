namespace Nop.Plugin.Baramjk.OrderPaymentLink.Services.Model
{
    public class GetOrderPaymentUrlResponseModel
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string PaymentUrl { get; set; }
    }
}