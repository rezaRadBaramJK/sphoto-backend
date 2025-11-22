using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Plugin.Baramjk.Framework.Services.Sms;
using Nop.Plugin.Baramjk.PushNotification.Factories.Admin;
using Nop.Plugin.Baramjk.PushNotification.Factories.Api;
using Nop.Plugin.Baramjk.PushNotification.Services;
using Nop.Plugin.Baramjk.PushNotification.Services.Providers;
using Nop.Plugin.Baramjk.PushNotification.Services.Providers.WhatsApp;

namespace Nop.Plugin.Baramjk.PushNotification.Plugins
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            //services
            services.AddScoped<IPushNotificationTokenService, TokenService>();
            services.AddScoped<LegacyPushNotificationSenderService>();
            services.AddScoped<Version1FcmPushNotificationSenderService>();
            services.AddTransient<IPushNotificationSenderService>(provider =>
            {
                var settings = provider.GetService<PushNotificationSettings>();
                if (settings.Strategy == 1)
                    return provider.GetService<Version1FcmPushNotificationSenderService>();

                return provider.GetService<LegacyPushNotificationSenderService>();
            });
            services.AddScoped<IPushEventHandlerService, SmsEventHandler>();
            services.AddScoped<IMobileService, MobileService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<EventPushService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<PushNotificationScheduleTaskService>();
            services.AddTransient<EventNotificationConfigService>();
            services.AddTransient<PushNotificationCustomerService>();
            services.AddTransient<ScheduleNotificationService>();
            services.AddTransient<CustomerNotifyProfileService>();
            //providers
            services.AddTransient<IWhatsAppProvider, RmlWhatsAppProvider>();
            services.AddTransient<IWhatsAppProvider, TwilioWhatsAppProvider>();

            services.AddTransient<ISmsProvider, RmlConnectSmsProvider>();
            services.AddTransient<ISmsProvider, FCCSmsProvider>();

            //factories
            services.AddSingleton<ISmsServiceFactory, SmsServiceFactory>();
            services.AddTransient<SettingFactory>();
            services.AddTransient<EventNotificationConfigFactory>();
            services.AddTransient<CustomerNotifyProfileFactory>();
        }

        public int Order => 1;
    }
}