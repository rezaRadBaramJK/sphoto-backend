using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Plugin.Baramjk.PhotoPlatform.Services.ContactUs;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Plugins
{
    public class Troubleshoot : TroubleshootBase


    {
        private readonly PhotoPlatformContactUsMessageTemplateService _photoPlatformContactUsMessageTemplateService;
        private readonly PhotoPlatformCustomerService _photoPlatformCustomerService;

        public Troubleshoot(PhotoPlatformContactUsMessageTemplateService photoPlatformContactUsMessageTemplateService,
            PhotoPlatformCustomerService photoPlatformCustomerService)
        {
            _photoPlatformContactUsMessageTemplateService = photoPlatformContactUsMessageTemplateService;
            _photoPlatformCustomerService = photoPlatformCustomerService;
        }

        public override async Task TroubleshootAsync()
        {
            await LogAsync("Start Photo Platform Troubleshoot", "");
            await AddLocaleResourceAsync(PhotoPlatformPlugin.Localizations, 1);
            await TroubleshootMigrationAsync();
            await InstallPermissionsAsync(new PermissionProvider());
            await _photoPlatformContactUsMessageTemplateService.AddContactOwnerPaymentSuccessfulMessageTemplateIfNotExistAsync();
            await _photoPlatformCustomerService.SubmitScheduleTaskAsync();
            await LogAsync("End Photo Platform Troubleshoot", "");
        }
    }
}