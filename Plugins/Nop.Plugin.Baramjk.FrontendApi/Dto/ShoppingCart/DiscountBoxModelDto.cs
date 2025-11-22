using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class DiscountBoxModelDto : ModelDto
    {
        public List<DiscountInfoModelDto> AppliedDiscountsWithCodes { get; set; }

        public bool Display { get; set; }

        public List<string> Messages { get; set; }

        public bool IsApplied { get; set; }
    }
}