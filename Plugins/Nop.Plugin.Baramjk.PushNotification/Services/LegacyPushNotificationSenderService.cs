using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core.Events;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Plugin.Baramjk.PushNotification.Plugins;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public class LegacyPushNotificationSenderService : IPushNotificationSenderService
    {
        private const string Url = "https://fcm.googleapis.com/fcm/send";
        
        private readonly IEventPublisher _eventPublisher;
        private readonly HttpClient _httpClient;
        private readonly PushNotificationSettings _pushNotificationSettings;
        private readonly ILogger _logger;

        public LegacyPushNotificationSenderService(
            PushNotificationSettings pushNotificationSettings,
            IEventPublisher eventPublisher,
            ILogger logger)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
            _pushNotificationSettings = pushNotificationSettings;
            _httpClient = new HttpClient();
            
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization",
                pushNotificationSettings.ServerKey);
        }

        public async Task SendAsync(Notification notification)
        {
            if (_pushNotificationSettings.DisableNotification)
            {
                await _logger.WarningAsync("Push notification is disabled.");
                return;
            }
            var splitList = SplitList(notification.RegistrationIds.ToList(), 1000);
            foreach (var smallTokenList in splitList)
            {
                var postModel = new
                {
                    registration_ids = smallTokenList,
                    data = notification.Data,
                    notification = new
                    {
                        body = notification.Body,
                        image = notification.Image,
                        title = notification.Title,
                        link = notification.Link,
                        clickAction = notification.ClickAction,
                        sound = "default",
                    }
                };

                await PostAsync(postModel);
            }

            await _eventPublisher.PublishAsync(notification);
        }

        public Task SendAsync(IList<Notification> notifications)
        {
            if (_pushNotificationSettings.DisableNotification)
            {
                _logger.WarningAsync("Push notification is disabled.");
                return Task.CompletedTask;
            }
            if(notifications.Any() == false)
                return Task.CompletedTask;
            
            var tasks = notifications.Select(SendAsync).ToList();
            return Task.WhenAll(tasks);
        }

        public async Task SendAsync(Notification notification, DateTime dateTime)
        {
            
        }

        private async Task PostAsync(object message)
        {
            var json = JsonConvert.SerializeObject(message);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(Url, stringContent);
        }

        private static IEnumerable<List<string>> SplitList(List<string> bigList, int nSize = 3)
        {
            for (var i = 0; i < bigList.Count; i += nSize)
                yield return bigList.GetRange(i, Math.Min(nSize, bigList.Count - i));
        }
    }
}