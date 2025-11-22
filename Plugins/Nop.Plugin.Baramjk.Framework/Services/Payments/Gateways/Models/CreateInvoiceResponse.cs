using Nop.Plugin.Baramjk.Framework.Services.Networks;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models
{
    public class CreateInvoiceResponse : IResponseModel
    {
        public bool IsSuccess { get; set; }
        public string InvoiceId { get; set; }
        public string PaymentUrl { get; set; }
        public string ClientReferenceId { get; set; }
        public string Message { get; set; }
    }
}