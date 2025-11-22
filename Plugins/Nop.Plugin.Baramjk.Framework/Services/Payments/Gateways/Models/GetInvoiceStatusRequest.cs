namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models
{
    public class GetInvoiceStatusRequest
    {
        public GetInvoiceStatusRequest()
        {
        }

        public GetInvoiceStatusRequest(string invoiceId)
        {
            InvoiceId = invoiceId;
        }

        public string InvoiceId { get; set; }
    }
}