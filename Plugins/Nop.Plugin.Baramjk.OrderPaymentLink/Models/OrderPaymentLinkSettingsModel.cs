using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.OrderPaymentLink.Models
{
    public class OrderPaymentLinkSettingsModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.OrderPaymentLink.Admin.Configuration.LinkExpireAfterMinutes")]
        public int LinkExpireAfterMinutes { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.OrderPaymentLink.Admin.Configuration.Message")]
        public string Message { get; set; }
    }
}