using Nop.Core.Configuration;

namespace Nop.Plugin.Baramjk.PushNotification.Plugins
{
    public class WhatsAppSettings: ISettings
    {
        public string ProviderName { get; set; }
        
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public string ApiSid { get; set; }
        
        public string ApiSecret { get; set; }
        
        public string SenderPhoneNumber { get; set; }
        
        public string OtpTemplateName { get; set; }
    }
}