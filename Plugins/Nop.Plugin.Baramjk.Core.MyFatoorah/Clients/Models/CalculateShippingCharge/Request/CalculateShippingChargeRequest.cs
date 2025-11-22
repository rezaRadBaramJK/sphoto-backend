using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.CalculateShippingCharge.Request
{
    public class CalculateShippingChargeRequest
    {
        public ShippingMethod ShippingMethod { get; set; }
        public string CityName { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public List<CalculateShippingChargeItem> Items { get; set; }
    }
}