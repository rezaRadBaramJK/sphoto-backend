using System;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Framework.Events.Payments.Gateways
{
    public class GatewayPaymentTranslationStatusEvent : GatewayPaymentTranslationEvent
    {
        public GatewayPaymentTranslationStatusEvent(GatewayPaymentTranslation entity, GatewayPaymentStatus oldStatus)
            : base(entity, EventKeys.GatewayPaymentTranslationStatus)
        {
            OldStatus = oldStatus;
        }

        public GatewayPaymentStatus OldStatus { get; set; }

        public bool JustPaid => OldStatus != GatewayPaymentStatus.Paid && Entity.Status == GatewayPaymentStatus.Paid;

        public ConsumerResult ConsumerResult { get; set; }
        public bool IsOwnerConsumerHandled { get;private set; }

        public void Handle(ConsumerResult result)
        {
            ConsumerResult = result ?? throw new ArgumentNullBusinessException(nameof(ConsumerResult));
            IsOwnerConsumerHandled = true;
        }
    }

    public class ConsumerResult
    {
        public ConsumerStatus ConsumerStatus { get; set; }  
        public object ConsumerPayload { get; set; }
        public string Message { get; set; }
    }
}