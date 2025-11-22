using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models.Suppliers
{
    public class SupplierProductPair
    {
        public int ProductId { get; set; }
        
        public Supplier Supplier { get; set; }
    }
}