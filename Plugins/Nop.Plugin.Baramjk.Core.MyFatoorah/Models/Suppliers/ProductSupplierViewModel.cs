using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models.Suppliers
{
    public record ProductSupplierViewModel: BaseNopEntityModel
    {
        public int ProductId { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Product.Supplier")]
        public int SupplierId { get; set; }

        public IList<SelectListItem> AvailableSuppliers { get; set; } = new List<SelectListItem>();

    }
}