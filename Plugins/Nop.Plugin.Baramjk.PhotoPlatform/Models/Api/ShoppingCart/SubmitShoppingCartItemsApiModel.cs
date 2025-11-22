using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ShoppingCart
{
    public class SubmitShoppingCartItemsApiModel
    {
        public SubmitShoppingCartItemsApiModel(List<SubmitShoppingCartItemApiModel> items)
        {
            Items = items;
        }

        public List<SubmitShoppingCartItemApiModel> Items { get; set; }
    }
}