using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.Abstractions;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.SendPayment
{
    public class SendPaymentRequest : PaymentRequestBase
    {
        public string NotificationOption { get; set; }
    }
}