using System.Collections.Generic;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Common;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.SendPayment
{
    public class SendPaymentFullDataRequest : SendPaymentRequest
    {
        public string MobileCountryCode { get; set; }
        public string Language { get; set; }
        public string SourceInfo { get; set; }
        public CustomerAddress CustomerAddress { get; set; }
        public List<InvoiceItem> InvoiceItems { get; set; }
        public ShippingMethod ShippingMethod { get; set; }
        public ShippingConsignee ShippingConsignee { get; set; }
    }
}