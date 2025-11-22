using Nop.Plugin.Baramjk.Framework.Domain;

namespace Nop.Plugin.Baramjk.Framework.Events.Payments.Gateways
{
    public class GatewayPaymentTranslationCreateInvoiceEvent : GatewayPaymentTranslationEvent
    {
        public GatewayPaymentTranslationCreateInvoiceEvent(GatewayPaymentTranslation entity, bool isSuccess)
            : base(entity, EventKeys.GatewayPaymentTranslationCreateInvoice)
        {
        }

        public bool IsSuccess { get; set; }
    }
}