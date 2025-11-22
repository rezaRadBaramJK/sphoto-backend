using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class ShippingMethodModelDto : ModelDto
    {
        public string ShippingRateComputationMethodSystemName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Fee { get; set; }
        public bool Selected { get; set; }

        public ShippingOptionDto ShippingOption { get; set; }
    }
}