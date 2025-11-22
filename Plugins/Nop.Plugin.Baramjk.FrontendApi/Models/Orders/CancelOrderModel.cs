namespace Nop.Plugin.Baramjk.FrontendApi.Models.Orders
{
    public class CancelOrderModel
    {
        public int OrderId { get; set; }
        public string Note { get; set; }
        public bool NotifyCustomer { get; set; }
    }
}