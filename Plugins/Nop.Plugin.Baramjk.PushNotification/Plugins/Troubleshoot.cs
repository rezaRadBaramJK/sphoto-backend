using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;
using Nop.Plugin.Baramjk.PushNotification.Services;

namespace Nop.Plugin.Baramjk.PushNotification.Plugins
{
    public class Troubleshoot : TroubleshootBase
    {
        private readonly PushNotificationScheduleTaskService _pushNotificationScheduleTaskService;
        private readonly EventNotificationConfigService _eventNotificationConfigService;

        public Troubleshoot(
            PushNotificationScheduleTaskService pushNotificationScheduleTaskService,
            EventNotificationConfigService eventNotificationConfigService)
        {
            _pushNotificationScheduleTaskService = pushNotificationScheduleTaskService;
            _eventNotificationConfigService = eventNotificationConfigService;
        }

        public override async Task TroubleshootAsync()
        {
            await LogAsync("Start PushNotification Troubleshoot", "");
            await TroubleshootSettingAsync(PushNotificationPlugin.GetDefaultSetting);
            await AddLocaleResourceAsync(PushNotificationPlugin.GetLocalizationResources);
            await TroubleshootMigrationAsync();
            await InstallPermissionsAsync(new PermissionProvider());
            await _eventNotificationConfigService.InitAsync();
            await _pushNotificationScheduleTaskService.Init();
            var query = @"IF COL_LENGTH (N'CustomerPushNotificationToken', N'Device') IS NOT NULL
                        BEGIN
                            ALTER TABLE CustomerPushNotificationToken
                            DROP COLUMN Device
                        END";
            await ExecuteNonQueryAsync(query);

            await LogAsync("End PushNotification Troubleshoot", "");
        }
    }
}