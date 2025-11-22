using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class CheckoutShippingAddressModelDto : ModelDto
    {
        public IList<AddressModelDto> ExistingAddresses { get; set; }
        public IList<AddressModelDto> InvalidExistingAddresses { get; set; }
        public AddressModelDto ShippingNewAddress { get; set; }
        public bool NewAddressPreselected { get; set; }

        public bool DisplayPickupInStore { get; set; }
        public CheckoutPickupPointsModelDto PickupPointsModel { get; set; }
    }
}