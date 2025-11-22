using System.Threading.Tasks;
using Nop.Plugin.Baramjk.ContactUs.Services;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;

namespace Nop.Plugin.Baramjk.ContactUs.Plugin
{
    public class Troubleshoot: TroubleshootBase
    {
        private readonly ContactUsMessageTemplateService _contactUsMessageTemplateService;

        public Troubleshoot(ContactUsMessageTemplateService contactUsMessageTemplateService)
        {
            _contactUsMessageTemplateService = contactUsMessageTemplateService;
        }

        public override async Task TroubleshootAsync()
        {
            await LogAsync("Start ContactUs Troubleshoot", "");
            
            await InstallPermissionsAsync(new PermissionProvider());
            await AddLocaleResourceAsync(ContactUsPlugin.Localization);
            await TroubleshootMigrationAsync();
            await TroubleshootSettingAsync(ContactUsPlugin.DefaultSettings);
            await _contactUsMessageTemplateService.AddContactOwnerPaymentSuccessfulMessageTemplateIfNotExistAsync();
            await LogAsync("End ContactUs Troubleshoot", "");
        }
    }
}