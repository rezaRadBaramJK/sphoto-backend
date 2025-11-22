using Nop.Core.Configuration;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Settings
{
    public class PhotoPlatformContactUsSettings: ISettings
    {
        public string PaymentCallbackUrl { get; set; }
        
        public bool NotifyAdminAfterPayment { get; set; }
        
        public string NotifyAdminEmail { get; set; }
        
        public string OwnerAdminEmail { get; set; }
        
        public string OwnerPhoneNumber { get; set; }
        
    }
}