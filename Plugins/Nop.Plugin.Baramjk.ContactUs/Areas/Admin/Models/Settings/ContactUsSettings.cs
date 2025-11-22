using Nop.Core.Configuration;

namespace Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Models.Settings
{
    public class ContactUsSettings: ISettings
    {
        public string PaymentCallbackUrl { get; set; }
        
        public bool NotifyAdminAfterPayment { get; set; }
        
        public string NotifyAdminEmail { get; set; }
        
        public string OwnerAdminEmail { get; set; }
        
        public string OwnerPhoneNumber { get; set; }
        
    }
}