using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework.Events;
using Nop.Plugin.Baramjk.Framework.Events.Orders;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;

namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public class EventPushService 
    {
        private readonly IPushNotificationSenderService _pushNotificationService;
        private readonly IPushNotificationTokenService _pushNotificationTokenService;

        public EventPushService(IPushNotificationSenderService pushNotificationService,
            IPushNotificationTokenService pushNotificationTokenService)
        {
            _pushNotificationService = pushNotificationService;
            _pushNotificationTokenService = pushNotificationTokenService;
        }

        private async Task<List<string>> GetAdminTokens()
        {
            return await _pushNotificationTokenService.GetTokensByRoleNameAsync(NopCustomerDefaults
                .AdministratorsRoleName);
        }

        public async Task HandleEventAsync(OrderPlacedEvent eventMessageEntity)
        {
            var tokens = await GetAdminTokens();
            const string title = "New order placed";
            var body = "New order placed. Order Id:" + eventMessageEntity.Order.Id;

            var notification = new Notification
            {
                RegistrationIds = tokens,
                Title = title,
                Body = body,
                Image = "",
                Data = new PushNotificationData
                {
                    Code = eventMessageEntity.Order.Id.ToString(),
                    NotificationType = NotificationTypes.Normal,
                    NotificationScopeType = NotificationScopeType.Admin
                }
            };

            await _pushNotificationService.SendAsync(notification);
        }

        public async Task HandleEventAsync(OrderNote eventMessageEntity)
        {
            var tokens = await GetAdminTokens();
            var title = "New order note";
            var body = eventMessageEntity.Note;

            var notification = new Notification
            {
                RegistrationIds = tokens,
                Title = title,
                Body = body,
                Image = "",
                Data = new PushNotificationData
                {
                    Code = eventMessageEntity.OrderId.ToString(),
                    NotificationType = NotificationTypes.Normal
                }
            };

            await _pushNotificationService.SendAsync(notification);
        }

        public async Task HandleEventAsync(OrderStatusBaseEvent eventMessageEntity)
        {
            if (eventMessageEntity.Name != EventKeys.OrderOrderStatusKey)
                return;

            var tokens = await GetAdminTokens();
            const string title = "Order status changed";
            var body = "Order status changed to " + eventMessageEntity.Entity.OrderStatus;

            var notification = new Notification
            {
                RegistrationIds = tokens,
                Title = title,
                Body = body,
                Image = "",
                Data = new PushNotificationData
                {
                    Code = eventMessageEntity.Entity.Id.ToString(),
                    NotificationType = NotificationTypes.Normal
                }
            };

            await _pushNotificationService.SendAsync(notification);
        }
    }
}