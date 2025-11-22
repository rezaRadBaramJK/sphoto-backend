using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;
using Nop.Plugin.Baramjk.FrontendApi.Infrastructure;
using Nop.Plugin.Baramjk.FrontendApi.Services;

namespace Nop.Plugin.Baramjk.FrontendApi
{
    public class Troubleshoot: TroubleshootBase
    {
        private readonly FrontendApiMessageTemplateService _frontendApiMessageTemplateService;

        public Troubleshoot(FrontendApiMessageTemplateService frontendApiMessageTemplateService)
        {
            _frontendApiMessageTemplateService = frontendApiMessageTemplateService;
        }

        public override async Task TroubleshootAsync()
        {
            await LogAsync("Start FrontendApi Troubleshoot", "");
            
            await AddLocaleResourceAsync(FrontendApiPlugin.Localization);
            await InstallPermissionsAsync(new PermissionProvider());
            await TroubleshootMigrationAsync();
            await _frontendApiMessageTemplateService.InitTemplatesAsync();
            
            await LogAsync("End FrontendApi Troubleshoot", "");
        }
    }
}