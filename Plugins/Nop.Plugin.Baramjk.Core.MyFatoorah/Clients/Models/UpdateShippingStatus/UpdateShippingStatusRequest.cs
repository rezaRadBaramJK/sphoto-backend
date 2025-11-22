using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.UpdateShippingStatus
{
    public class UpdateShippingStatusRequest
    {
        public int ShippingMethod { get; set; }
        public List<int> InvoiceNumbers { get; set; }
        public int OrderStatusChangedTo { get; set; }
    }
}