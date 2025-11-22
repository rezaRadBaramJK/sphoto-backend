using Nop.Core.Configuration;

namespace Nop.Plugin.Baramjk.OrderPaymentLink.Services.Model
{
    public class OrderPaymentLinkSetting : ISettings
    {
        public int LinkExpireAfterMinutes { get; set; }
        public string Message { get; set; }
    }
}