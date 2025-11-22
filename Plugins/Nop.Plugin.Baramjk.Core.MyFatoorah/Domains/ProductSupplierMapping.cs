using Nop.Core;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Domains
{
    public class ProductSupplierMapping: BaseEntity
    {
        public int ProductId { get; set; }
        
        public int SupplierId { get; set; }
    }
}