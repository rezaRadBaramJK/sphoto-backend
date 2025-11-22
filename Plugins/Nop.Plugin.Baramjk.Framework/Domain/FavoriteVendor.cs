using Nop.Core;

namespace Nop.Plugin.Baramjk.Framework.Domain
{
    public class FavoriteVendor : BaseEntity
    {
        public int CustomerId { get; set; }
        public int VendorId { get; set; }
    }
}