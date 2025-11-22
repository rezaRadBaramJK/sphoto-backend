namespace Nop.Plugin.Baramjk.Framework.Services.Currencies
{
    public class CurrencyConvertRequest
    {
        public decimal Amount { get; set; }
        public string FromCode { get; set; }
        public string ToCode { get; set; }
    }
}