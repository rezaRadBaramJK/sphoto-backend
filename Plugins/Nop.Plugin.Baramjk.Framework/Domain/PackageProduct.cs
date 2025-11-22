using Nop.Core;

namespace Nop.Plugin.Baramjk.Framework.Domain
{
    public class PackageProduct : BaseEntity
    {
        public int ParentProductId { get; set; }//The parent product which represents the package
        public int ProductId { get; set; } // The child product which represents the sub product
        public int Count { get; set; }
        public int DisplayOrder { get; set; }
    }
}