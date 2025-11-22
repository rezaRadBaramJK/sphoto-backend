namespace Nop.Plugin.Baramjk.Framework.Services.PushNotifications
{
    public class PushNotificationData
    {
        public string NotificationType { get; set; }
        public string Link { get; set; }
        public string Code { get; set; }
        public string ExtraData { get; set; }
        public NotificationScopeType NotificationScopeType { get; set; }
    }
}