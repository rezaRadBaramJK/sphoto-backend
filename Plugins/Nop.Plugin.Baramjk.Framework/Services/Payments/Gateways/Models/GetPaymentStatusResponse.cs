using System;
using Nop.Plugin.Baramjk.Framework.Services.Networks;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models
{
    public class InvoiceStatusResponse:IResponseModel
    {
        public bool IsSuccess { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceStatus { get; set; }
        public string InvoiceReference { get; set; }
        public string ClientReferenceId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public decimal Amount { get; set; }
        public string Message { get; set; }
        public bool IsPaid { get; set; }
        public GatewayPaymentStatus PaymentStatus { get; set; }
    }
}