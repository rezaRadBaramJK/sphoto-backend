using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Settings
{
    public record ContactUsSettingsViewModel: BaseNopModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Settings.PaymentCallbackUrl")]
        public string PaymentCallbackUrl { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.NotifyAdminAfterPayment")]
        public bool NotifyAdminAfterPayment { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.NotifyAdminEmail")]
        public string NotifyAdminEmail { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.OwnerAdminEmail")]
        public string OwnerAdminEmail { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.OwnerPhoneNumber")]
        public string OwnerPhoneNumber { get; set; }
    }
}