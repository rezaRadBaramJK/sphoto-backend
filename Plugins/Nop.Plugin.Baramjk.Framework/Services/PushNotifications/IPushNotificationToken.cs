using System;

namespace Nop.Plugin.Baramjk.Framework.Services.PushNotifications
{
    public interface IPushNotificationToken
    {
        int Id { get; set; }
        int CustomerId { get; set; }
        string Token { get; set; }
        public bool IsActive { get; set; }
        NotificationPlatform Platform { get; set; }
        DateTime LastModify { get; set; }
    }
}