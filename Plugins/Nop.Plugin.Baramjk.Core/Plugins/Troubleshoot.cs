using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;

namespace Nop.Plugin.Baramjk.Core.Plugins
{
    public class Troubleshoot : TroubleshootBase
    {
        public override async Task TroubleshootAsync()
        {
            await LogAsync("Start Core Troubleshoot", "");
            await TroubleshootSettingAsync(BaramjkCorePlugin.GetDefaultFrameworkSetting);
            await InstallPermissionsAsync(new PermissionProvider());
            await TroubleshootMigrationAsync();
            await AddLocaleResourceAsync(BaramjkCorePlugin.Localizations);
            await LogAsync("End Core Troubleshoot", "");
        }
    }
}