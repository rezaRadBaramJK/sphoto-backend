using Nop.Core;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Domains
{
    public class TransactionSupplier: BaseEntity
    {
        public int NopSupplierId { get; set; }
        
        public int TransactionId { get; set; }
        
        public decimal InvoiceShare { get; set; }
    }
}