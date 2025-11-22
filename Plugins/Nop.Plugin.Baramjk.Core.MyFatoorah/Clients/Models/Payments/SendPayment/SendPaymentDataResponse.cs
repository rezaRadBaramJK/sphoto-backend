using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.Abstractions;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.SendPayment
{
    public class SendPaymentDataResponse: PaymentDataBaseResponse
    {
        public string InvoiceURL { get; set; }

        public override string Url => InvoiceURL;
    }
}