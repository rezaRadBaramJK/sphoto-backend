namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.Abstractions
{
    public abstract class PaymentDataBaseResponse
    {
        public long InvoiceId { get; set; }
        
        public abstract string Url { get; }
        
        public string CustomerReference { get; set; }

        public string UserDefinedField { get; set; }
    }
}