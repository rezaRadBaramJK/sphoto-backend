using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.EventDetails
{
    public class ProductEventDetails
    {
        public Product Product { get; set; }
        
        public EventDetail EventDetails { get; set; }
    }
}