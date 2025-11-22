using System;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;

namespace Nop.Plugin.Baramjk.PushNotification.Domain
{
    public class PushNotificationToken : BaseEntity, IPushNotificationToken
    {
        public int CustomerId { get; set; }
        public string Token { get; set; }
        public bool IsActive { get; set; }
        public NotificationPlatform Platform { get; set; }
        public DateTime LastModify { get; set; }
    }
}