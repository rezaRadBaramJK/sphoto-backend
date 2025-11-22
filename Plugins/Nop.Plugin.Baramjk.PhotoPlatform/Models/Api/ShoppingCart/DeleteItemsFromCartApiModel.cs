using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ShoppingCart
{
    public class DeleteItemsFromCartApiModel
    {
        public DeleteItemsFromCartApiModel(List<DeleteItemFromCartApiModel> items)
        {
            Items = items;
        }

        public List<DeleteItemFromCartApiModel> Items { get; set; }
    }
}