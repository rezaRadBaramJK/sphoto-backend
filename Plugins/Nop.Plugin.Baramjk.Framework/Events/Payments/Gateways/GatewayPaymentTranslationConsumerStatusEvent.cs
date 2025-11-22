using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Framework.Events.Payments.Gateways
{
    public class GatewayPaymentTranslationConsumerStatusEvent : GatewayPaymentTranslationEvent
    {
        public GatewayPaymentTranslationConsumerStatusEvent(GatewayPaymentTranslation entity, ConsumerStatus oldStatus)
            : base(entity, EventKeys.GatewayPaymentTranslationConsumerStatus)
        {
            OldStatus = oldStatus;
        }

        public ConsumerStatus OldStatus { get; set; }
    }
}