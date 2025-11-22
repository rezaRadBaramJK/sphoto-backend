using Nop.Core.Configuration;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins
{
    public class MyFatoorahSettings : ISettings
    {
        public string MyFatoorahAccessKey { get; set; }
        
        public string WebhookSecretKey { get; set; }
        
        public bool EnableRedirect { get; set; }

        public string? BackendBase { get; set; }
        public string? FrontendBase { get; set; }
        public string? SuccessFrontendCallback { get; set; }
        public string? FailedFrontendCallback { get; set; }
        public bool MyFatoorahUseSandbox { get; set; }
        public string DisplayCurrencyIsoAlpha { get; set; }
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFeePercentage { get; set; }
        
        public int DefaultSupplierId { get; set; }
        
        public bool SkipPaymentInfo { get; set; }
        public bool ChargeOnCustomer { get; set; }
        
    }
}