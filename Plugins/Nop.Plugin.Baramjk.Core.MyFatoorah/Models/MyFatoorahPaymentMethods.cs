namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models
{
    public class MyFatoorahPaymentMethods
    {
        public int PaymentMethodId { get; set; }
        public string PaymentMethodAr { get; set; }
        public string PaymentMethodEn { get; set; }
        public string PaymentMethodCode { get; set; }
        public bool IsDirectPayment { get; set; }
        public double ServiceCharge { get; set; }
        public double TotalAmount { get; set; }
        public string CurrencyIso { get; set; }
        public string ImageUrl { get; set; }
        public bool IsEmbeddedSupported { get; set; }
        public string PaymentCurrencyIso { get; set; }
    }
}