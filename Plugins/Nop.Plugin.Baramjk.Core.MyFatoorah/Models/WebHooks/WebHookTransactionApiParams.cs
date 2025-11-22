using System;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models.WebHooks
{
    public class WebHookTransactionApiParams
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentId { get; set; }
        public string ReferenceId { get; set; }
        public string TrackId { get; set; }
        public string AuthorizationId { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}