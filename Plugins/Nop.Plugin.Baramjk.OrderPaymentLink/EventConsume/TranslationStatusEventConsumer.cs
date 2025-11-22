using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Events.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Plugin.Baramjk.OrderPaymentLink.Services;
using Nop.Services.Events;

namespace Nop.Plugin.Baramjk.OrderPaymentLink.EventConsume
{
    public class TranslationStatusEventConsumer : IConsumer<GatewayPaymentTranslationStatusEvent>
    {
        private readonly IPushNotificationSenderService _pushNotificationService;
        private readonly IPushNotificationTokenService _pushNotificationTokenService;

        public TranslationStatusEventConsumer(IPushNotificationSenderService pushNotificationService,
            IPushNotificationTokenService pushNotificationTokenService)
        {
            _pushNotificationService = pushNotificationService;
            _pushNotificationTokenService = pushNotificationTokenService;
        }

        public async Task HandleEventAsync(GatewayPaymentTranslationStatusEvent eventMessage)
        {
            var translation = eventMessage.Entity;
            if (translation.ConsumerName != InvoiceService.ConsumerName)
                return;

            var tokens = await _pushNotificationTokenService.GetTokensByRoleNameAsync(NopCustomerDefaults
                .AdministratorsRoleName);

            var payedState = translation.Status == GatewayPaymentStatus.Paid ? "Paid" : "Not Success";
            const string title = "Order payment by link";
            var body = $"Order payment by link was {payedState} orderId:{translation.ConsumerEntityId}";

            var notification = new Notification
            {
                RegistrationIds = tokens,
                Title = title,
                Body = body,
                Image = "",
                Data = new PushNotificationData
                {
                    NotificationType = NotificationTypes.Normal,
                    NotificationScopeType = NotificationScopeType.Admin
                }
            };

            await _pushNotificationService.SendAsync(notification);
        }
    }
}