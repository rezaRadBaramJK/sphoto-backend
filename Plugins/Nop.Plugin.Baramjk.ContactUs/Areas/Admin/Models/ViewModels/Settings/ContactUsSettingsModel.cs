using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Models.ViewModels.Settings
{
    public record ContactUsSettingsModel: BaseNopModel
    {
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.Settings.PaymentCallbackUrl")]
        public string PaymentCallbackUrl { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.Subjects.NotifyAdminAfterPayment")]
        public bool NotifyAdminAfterPayment { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.Subjects.NotifyAdminEmail")]
        public string NotifyAdminEmail { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.Subjects.OwnerAdminEmail")]
        public string OwnerAdminEmail { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.Subjects.OwnerPhoneNumber")]
        public string OwnerPhoneNumber { get; set; }
    }
}