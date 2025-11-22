using Nop.Plugin.Baramjk.Framework.Domain;

namespace Nop.Plugin.Baramjk.Framework.Events.Payments.Gateways
{
    public class GatewayPaymentTranslationEvent : BaramjkEvent<GatewayPaymentTranslation>
    {
        public GatewayPaymentTranslationEvent(GatewayPaymentTranslation entity) : base(entity)
        {
        }

        public GatewayPaymentTranslationEvent(GatewayPaymentTranslation entity, string name) : base(entity, name)
        {
        }
    }
}