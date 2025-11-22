using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class CheckoutBillingAddressModelDto : ModelDto
    {
        public IList<AddressModelDto> ExistingAddresses { get; set; }
        public IList<AddressModelDto> InvalidExistingAddresses { get; set; }

        public AddressModelDto BillingNewAddress { get; set; }

        public bool ShipToSameAddress { get; set; }
        public bool ShipToSameAddressAllowed { get; set; }

        /// <summary>
        ///     Used on one-page checkout page
        /// </summary>
        public bool NewAddressPreselected { get; set; }
    }
}