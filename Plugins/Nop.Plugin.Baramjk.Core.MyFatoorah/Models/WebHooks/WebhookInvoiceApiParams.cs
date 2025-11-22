using System;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models.WebHooks
{
    public class WebhookInvoiceApiParams
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Reference { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string UserDefinedField { get; set; }
        public string ExternalIdentifier { get; set; }
    }
}