using Nop.Core;

namespace Nop.Plugin.Baramjk.PushNotification.Domain
{
    public class EventNotificationConfigEntity: BaseEntity
    {
        public string EventName { get; set; }
        
        public string StatusName { get; set; }
        
        public string TemplateName { get; set; }
        
        public bool UseSms { get; set; }
        
        public bool UseFirebase { get; set; }
        
        public bool UseWhatsApp { get; set; }
    }
}