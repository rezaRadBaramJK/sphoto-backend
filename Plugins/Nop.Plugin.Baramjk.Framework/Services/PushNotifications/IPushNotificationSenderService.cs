using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.Framework.Services.PushNotifications
{
    public interface IPushNotificationSenderService
    {
        Task SendAsync(Notification notification);

        Task SendAsync(IList<Notification> notifications);

        //Scheduled
        Task SendAsync(Notification notification, DateTime dateTime);
    }
}