using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class OrderReviewDataModelDto : ModelDto
    {
        public bool Display { get; set; }

        public AddressModelDto BillingAddress { get; set; }

        public bool IsShippable { get; set; }

        public AddressModelDto ShippingAddress { get; set; }

        public bool SelectedPickupInStore { get; set; }

        public AddressModelDto PickupAddress { get; set; }

        public string ShippingMethod { get; set; }

        public string PaymentMethod { get; set; }

        public Dictionary<string, object> CustomValues { get; set; }
    }
}