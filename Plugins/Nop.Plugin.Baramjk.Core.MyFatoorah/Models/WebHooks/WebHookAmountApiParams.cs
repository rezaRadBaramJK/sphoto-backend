namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models.WebHooks
{
    public class WebHookAmountApiParams
    {
        public string BaseCurrency { get; set; }
        public decimal ValueInDisplayCurrency { get; set; }
        public string PayCurrency { get; set; }
        public decimal ValueInPayCurrency { get; set; }
        
        
    }
}