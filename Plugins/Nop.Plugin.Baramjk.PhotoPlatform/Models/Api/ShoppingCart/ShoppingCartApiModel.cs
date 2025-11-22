using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;


namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ShoppingCart
{
    public class ShoppingCartApiModel
    {
        public Product Product { get; set; }

        public ShoppingCartItem ShoppingCartItem { get; set; }

        public TimeSlot TimeSlot { get; set; }

        public ShoppingCartItemTimeSlot ShoppingCartItemTimeSlot { get; set; }

        public Actor Actor { get; set; }
    }
}