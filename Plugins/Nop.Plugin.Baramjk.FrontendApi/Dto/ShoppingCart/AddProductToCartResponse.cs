using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class AddProductToCartResponse : BaseDto
    {
        public IList<string> Errors { get; set; }

        public bool Success { get; set; }

        public string Message { get; set; }

        public MiniShoppingCartModelDto Model { get; set; }
    }
}