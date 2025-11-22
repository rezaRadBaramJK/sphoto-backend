namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models.WebHooks
{
    public class WebHookDataApiParams
    {
        public WebhookInvoiceApiParams Invoice { get; set; }
        public WebHookTransactionApiParams Transaction { get; set; }
        public WebHookAmountApiParams Amount { get; set; }
    }
}