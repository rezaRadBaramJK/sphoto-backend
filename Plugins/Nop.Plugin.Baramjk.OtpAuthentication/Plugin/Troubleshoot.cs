using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;
using Nop.Plugin.Baramjk.OtpAuthentication.Services;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Plugin
{
    public class Troubleshoot: TroubleshootBase
    {
        private readonly OtpMessageTemplateService _otpMessageTemplateService;

        public Troubleshoot(OtpMessageTemplateService otpMessageTemplateService)
        {
            _otpMessageTemplateService = otpMessageTemplateService;
        }

        public override async Task TroubleshootAsync()
        {
            await LogAsync("Start OtpAuthentication Troubleshoot", "");
            await TroubleshootSettingAsync(OtpAuthenticationPlugin.DefaultSetting);
            await TroubleshootMigrationAsync();
            await InstallPermissionsAsync(new PermissionProvider());
            await AddLocaleResourceAsync(OtpAuthenticationPlugin.LocalizationResources);
            await _otpMessageTemplateService.AddOtpMessageTemplateIfNotExistAsync();
            await LogAsync("End OtpAuthentication Troubleshoot", "");
        }
    }
}