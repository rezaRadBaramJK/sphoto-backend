using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Domains
{
    public class Supplier: BaseEntity, ILocalizedEntity, ISoftDeletedEntity
    {
        public string Name { get; set; }
        
        public int SupplierCode { get; set; }

        public string Mobile { get; set; }
        
        public string Email { get; set; }
        
        public double CommissionValue { get; set; }
        
        public double CommissionPercentage { get; set; }
        
        public string Status { get; set; }

        public bool Deleted { get; set; }

        public override bool Equals(object obj)
        { 
            if(obj is Supplier supplier)
            {
                return supplier.Id == Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return SupplierCode.GetHashCode();
        }
    }
}