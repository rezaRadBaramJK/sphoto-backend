using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Plugin.Baramjk.PushNotification.Services;
using Nop.Services.Events;

namespace Nop.Plugin.Baramjk.PushNotification.EventConsume
{
    public class NotificationEventConsumer : IConsumer<Notification>
    {
        private readonly NotificationService _notificationService;

        public NotificationEventConsumer(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task HandleEventAsync(Notification eventMessage)
        {
            await _notificationService.SaveNotificationsAsync(eventMessage);
        }
    }
}