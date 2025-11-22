using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Event
{
    public class BookmarkedEventModel
    {
        public Product Product { get; set; }
        public EventDetail EventDetail { get; set; }
        public ShoppingCartItem ShoppingCartItem { get; set; }
    }
}