using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.Abstractions;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.ExecutePayment
{
    public class ExecutePaymentRequest : PaymentRequestBase
    {
        public int PaymentMethodId { get; set; }
        
        public string Language { get; set; }
    }
}