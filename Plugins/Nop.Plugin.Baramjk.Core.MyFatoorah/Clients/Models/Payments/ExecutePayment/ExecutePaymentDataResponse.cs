using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.Abstractions;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.ExecutePayment
{
    public class ExecutePaymentDataResponse: PaymentDataBaseResponse
    {
        public bool IsDirectPayment { get; set; }
        
        public string PaymentURL { get; set; }
        
        public string RecurringId { get; set; }

        public override string Url => PaymentURL;
    }
}