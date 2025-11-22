using System;
using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Domain;

namespace Nop.Plugin.Baramjk.OrderPaymentLink.Services.Model
{
    public class ReportItem
    {
        public int OrderId { get; set; }
        public decimal OrderTotal { get; set; }
        public int TransactionCount { get; set; }
        public List<GatewayPaymentTranslation> Transactions { get; set; }
        public DateTime OrderCreatedOnUtc { get; set; }
        public string OrderStatus { get; set; }
        public string OrderPaymentStatus { get; set; }
    }
}