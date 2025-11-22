using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.GetShippingOrderList
{
    public class GetShippingOrderListResponse
    {
        public string ShippingMethod { get; set; }
        public string OrderStatus { get; set; }
        public int TotalOrders { get; set; }
        public List<int> OrderNumbers { get; set; }
        public List<ShippingOrder> ShippingOrders { get; set; }
    }
}