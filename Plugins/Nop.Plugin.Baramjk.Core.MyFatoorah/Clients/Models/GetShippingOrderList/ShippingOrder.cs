namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.GetShippingOrderList
{
    public class ShippingOrder
    {
        public int OrderNumber { get; set; }
        public string OrderType { get; set; }
        public string OrderStatus { get; set; }
        public string CustomerName { get; set; }
        public string ShippingMethod { get; set; }
        public decimal ShippingValue { get; set; }
    }
}