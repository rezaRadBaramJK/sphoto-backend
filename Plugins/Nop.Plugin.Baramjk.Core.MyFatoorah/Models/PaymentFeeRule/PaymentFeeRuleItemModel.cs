using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models.PaymentFeeRule
{
    public record PaymentFeeRuleItemModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.PaymentMethodName")]
        public string PaymentMethodName { get; set; }
        
        [NopResourceDisplayName("Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.CountryName")]
        public string CountryName { get; set; }
        
        [NopResourceDisplayName("Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.AdditionalFee")]
        public decimal AdditionalFee { get; set; }


        [NopResourceDisplayName("Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.Active")]
        public bool Active { get; set; }
    }
}