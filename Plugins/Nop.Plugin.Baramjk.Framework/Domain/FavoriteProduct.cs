using Nop.Core;

namespace Nop.Plugin.Baramjk.Framework.Domain
{
    public class FavoriteProduct : BaseEntity
    {
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
    }
}