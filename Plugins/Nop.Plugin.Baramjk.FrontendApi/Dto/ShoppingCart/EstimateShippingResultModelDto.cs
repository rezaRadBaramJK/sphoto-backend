using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class EstimateShippingResultModelDto : ModelDto
    {
        public IList<ShippingOptionModelDto> ShippingOptions { get; set; }

        public bool Success { get; set; }

        public IList<string> Errors { get; set; }
    }
}