using System.Linq;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Suppliers;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Extensions;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Factories
{
    public class SupplierFactory
    {
        public Supplier[] PrepareSuppliers(SupplierResponse[] supplierResponse)
        {
            return supplierResponse.Select(s => s.Map<Supplier>()).ToArray();
        }
    }
}