using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;

namespace Nop.Plugin.Baramjk.SocialLinks.Plugins
{
    public class Troubleshoot : TroubleshootBase
    {
        public override async Task TroubleshootAsync()
        {
            await LogAsync("Start SocialLinks Troubleshoot", "");
            await TroubleshootSettingAsync(SocialLinksPlugin.GetDefaultSetting);
            await InstallPermissionsAsync(new PermissionProvider());
            await LogAsync("End SocialLinks Troubleshoot", "");
        }
    }
}