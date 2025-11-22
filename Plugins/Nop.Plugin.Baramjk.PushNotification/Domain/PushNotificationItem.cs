using System;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;

namespace Nop.Plugin.Baramjk.PushNotification.Domain
{
    public class PushNotificationItem : BaseEntity
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Image { get; set; }
        public string NotificationType { get; set; }
        public string Link { get; set; }
        public string Code { get; set; }
        public string Data { get; set; }
        public string ExtraData { get; set; }
        public NotificationPlatform? NotificationPlatform { get; set; }
        public NotificationScopeType NotificationScopeType { get; set; }
        public DateTime OnDateTime { get; set; }
    }
}