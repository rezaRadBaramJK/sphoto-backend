using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models.Suppliers
{
    public record SupplierViewModel: BaseNopEntityModel, ILocalizedModel<SupplierLocalizationModel>
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Name")]
        public string Name { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.SupplierCode")]
        public int SupplierCode { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Mobile")]
        public string Mobile { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Email")]
        public string Email { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.CommissionValue")]
        public double CommissionValue { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.CommissionPercentage")]
        public double CommissionPercentage { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Status")]
        public string Status { get; set; }

        public IList<SupplierLocalizationModel> Locales { get; set; }
    }
}