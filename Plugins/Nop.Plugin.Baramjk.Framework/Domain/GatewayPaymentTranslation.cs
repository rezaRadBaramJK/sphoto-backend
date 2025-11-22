using System;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Framework.Domain
{
    public class GatewayPaymentTranslation : BaseEntity
    {
        public string Guid { get; set; }
        public string GatewayName { get; set; }
        public string MethodName { get; set; }
        public string PaymentUrl { get; set; }
        public string PaymentId { get; set; }
        public int PaymentOptionId { get; set; }
        public string InvoiceId { get; set; }
        public decimal AmountToPay { get; set; }
        public decimal AmountPayed { get; set; }
        public string Message { get; set; }
        public int OwnerCustomerId { get; set; }
        public string ConsumerName { get; set; }
        public string ConsumerEntityType { get; set; }
        public int ConsumerEntityId { get; set; }
        public string ConsumerData { get; set; }
        public string ConsumerCallBackUrl { get; set; }
        public int PaymentFeeRuleId { get; set; }
        public decimal PaymentFeeValue { get; set; }
        public ConsumerStatus ConsumerStatus { get; set; }
        public GatewayPaymentStatus Status { get; set; }
        public DateTime OnCreateDateTimeUtc { get; set; }
    }
}