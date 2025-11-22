namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Checkout
{
    public class ConfirmOrderBackendApiParams
    {
        public bool ProcessPayment { get; set; } = true;
        public string CustomerName { get; set; } 
        public string CustomerPhoneNumber { get; set; } 
        public string Device { get; set; } = "Mobile"; 
    }
}