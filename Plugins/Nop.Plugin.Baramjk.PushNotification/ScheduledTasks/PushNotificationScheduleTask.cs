using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Logging;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Baramjk.PushNotification.ScheduledTasks
{
    public class PushNotificationScheduleTask: IScheduleTask
    {
        private readonly IEnumerable<IPushNotificationCustomerProvider> _customerProviders;
        private readonly IPushNotificationSenderService _pushNotificationSenderService;
        private readonly IPushNotificationTokenService _pushNotificationTokenService;
        private readonly ILogger _logger;

        public PushNotificationScheduleTask(
            IEnumerable<IPushNotificationCustomerProvider> customerProviders, 
            IPushNotificationSenderService pushNotificationSenderService, 
            IPushNotificationTokenService pushNotificationTokenService,
            ILogger logger)
        {
            _customerProviders = customerProviders;
            _pushNotificationSenderService = pushNotificationSenderService;
            _pushNotificationTokenService = pushNotificationTokenService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            if (_customerProviders.Any() == false)
                return;

            var notifications = await _customerProviders.SelectManyAwait(async p =>
            {
                try
                {
                    var notifyCustomers=  await p.GetCustomersAsync();
                    notifyCustomers = notifyCustomers.Where(nc => nc.Customers.Any()).ToArray();
                
                    var notifyCustomersTokenPairs = await notifyCustomers.SelectAwait(async nc =>
                    {
                        var customerIds = nc.Customers.Select(c => c.Id);
                        return new
                        {
                            NofiyCustomers = nc,
                            Tokens = await _pushNotificationTokenService.GetTokensAsync(customerIds: customerIds)
                        };
                    }).ToArrayAsync();

                    notifyCustomersTokenPairs = notifyCustomersTokenPairs.Where(nct => nct.Tokens.Any() && nct.NofiyCustomers.Customers.Any()).ToArray();
                    return notifyCustomersTokenPairs.Select(nct => new Notification
                    {
                        RegistrationIds = nct.Tokens,
                        Title = nct.NofiyCustomers.Title,
                        Body = nct.NofiyCustomers.Body,
                        Image = "",
                        Data = new PushNotificationData
                        {
                            NotificationType = NotificationTypes.Normal,
                            NotificationScopeType = NotificationScopeType.Normal
                        }

                    });
                }
                catch (Exception e)
                {
                    await _logger.InsertLogAsync(LogLevel.Error, $"{p.GetType().FullName} - error in {nameof(PushNotificationScheduleTask)}", e.StackTrace);
                    return Enumerable.Empty<Notification>();
                }
                
            }).ToArrayAsync();
            
            if(notifications.Any() == false)
                return;
            
            await _pushNotificationSenderService.SendAsync(notifications);

        }
    }
}