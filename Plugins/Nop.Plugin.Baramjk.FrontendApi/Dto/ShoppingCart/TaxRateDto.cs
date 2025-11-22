using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class TaxRateDto : ModelDto
    {
        public string Rate { get; set; }

        public string Value { get; set; }
    }
}