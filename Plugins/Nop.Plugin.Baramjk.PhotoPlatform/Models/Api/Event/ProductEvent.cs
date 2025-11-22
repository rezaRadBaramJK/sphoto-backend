using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Event
{
    public class ProductEvent
    {
        public Product Product { get; set; }
        public EventDetail EventDetail { get; set; }

        public CashierEvent CashierEvent { get; set; }
    }
}