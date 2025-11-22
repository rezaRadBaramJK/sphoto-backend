using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class SelectShippingOptionResponse : BaseDto
    {
        public bool Success { get; set; }

        public OrderTotalsModelDto Model { get; set; }

        public List<string> Errors { get; set; }
    }
}