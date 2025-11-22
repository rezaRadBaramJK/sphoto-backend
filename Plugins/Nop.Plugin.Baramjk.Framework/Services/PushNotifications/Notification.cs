using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Framework.Services.PushNotifications
{
    public class Notification 
    {
        public Notification()
        {
            ClickAction = "FLUTTER_NOTIFICATION_CLICK";
        }

        public string Title { get; set; }
        public string Body { get; set; }
        public string Image { get; set; }
        public string ClickAction { get; set; }
        public string Link { get; set; }
        public object Data { get; set; }
        public string NotificationType { get; set; }
        public NotificationPlatform? NotificationPlatform { get; set; }
        public IEnumerable<string> RegistrationIds { get; set; }
    }
}