namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models
{
    public class InitiatePaymentRequest
    {
        public decimal InvoiceAmount { get; set; }
        public string CurrencyIso { get; set; }
    }
}