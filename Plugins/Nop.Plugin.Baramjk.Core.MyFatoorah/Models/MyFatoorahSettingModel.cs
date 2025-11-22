using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models
{
    public record MyFatoorahSettingModel : BaseNopModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.AccessKey")]
        public string MyFatoorahAccessKey { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.WebhookSecretKey")]
        public string WebhookSecretKey { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.UseSandbox")]
        public bool MyFatoorahUseSandbox { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.Redirect")]
        public bool EnableRedirect { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.FrontendRedirectBaseUrl")]
        public string? FrontendBase { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.BackendRedirectBaseUrl")]
        public string? BackendBase { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.SuccessFrontendCallback")]
        public string? SuccessFrontendCallback { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.FailedFrontendCallback")]
        public string? FailedFrontendCallback { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.DisplayCurrencyIsoAlpha")]
        public string DisplayCurrencyIsoAlpha { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.DefaultSupplierId")]
        public int DefaultSupplierId { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.SkipPaymentInfo")]
        public bool SkipPaymentInfo { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.ChargeOnCustomer")]
        public bool ChargeOnCustomer { get; set; }
        
        public IList<SelectListItem> AvailableSuppliers { get; set; } = new List<SelectListItem>();
    }
}