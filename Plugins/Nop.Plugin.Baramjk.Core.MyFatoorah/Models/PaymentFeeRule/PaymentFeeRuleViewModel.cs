using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models.PaymentFeeRule
{
    public record PaymentFeeRuleViewModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.PaymentMethodId")]
        public int PaymentMethodId { get; set; }
        [NopResourceDisplayName("Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.CountryId")]
        public int CountryId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.Active")]
        public bool Active { get; set; }
        
        
        public IList<SelectListItem> AvailableCountries { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem> AvailablePaymentMethods { get; set; } = new List<SelectListItem>();
        
     }
}