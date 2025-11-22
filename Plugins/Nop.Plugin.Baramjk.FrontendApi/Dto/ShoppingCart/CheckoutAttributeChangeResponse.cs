using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class CheckoutAttributeChangeResponse : BaseDto
    {
        public OrderTotalsModelDto OrderTotalsModel { get; set; }

        public string FormattedAttributes { get; set; }

        public int[] EnabledAttributeIds { get; set; }

        public int[] DisabledAttributeIds { get; set; }
    }
}