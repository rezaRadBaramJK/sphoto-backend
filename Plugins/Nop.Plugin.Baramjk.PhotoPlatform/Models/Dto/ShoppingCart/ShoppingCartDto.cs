using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.ShoppingCart
{
    public class ShoppingCartDto : CamelCaseBaseDto
    {
        public List<ShoppingCartDetailsDto> Items { get; set; }
        public string TotalPrice { get; set; }
    }
}