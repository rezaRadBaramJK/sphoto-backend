using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.GetPaymentStatus
{
    public class GetPaymentStatusRequest : GetInvoiceStatusRequest
    {
        public string Key => InvoiceId;
        public string KeyType { get; set; } = "InvoiceId";

        public static GetPaymentStatusRequest ByInvoiceId(string key)
        {
            return new GetPaymentStatusRequest
            {
                InvoiceId = key,
                KeyType = "InvoiceId"
            };
        }

        public static GetPaymentStatusRequest ByPaymentId(string key)
        {
            return new GetPaymentStatusRequest
            {
                InvoiceId = key,
                KeyType = "PaymentId"
            };
        }
    }
}