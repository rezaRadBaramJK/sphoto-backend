using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;

namespace Nop.Plugin.Baramjk.OrderPaymentLink.Plugins
{
    public class Troubleshoot : TroubleshootBase
    {
        public override async Task TroubleshootAsync()
        {
           
            await LogAsync("Start OrderPaymentLink Troubleshoot", "");
            await InstallPermissionsAsync(new PermissionProvider());
            await AddLocaleResourceAsync(OrderPaymentLinkPlugin.Localization);
            await LogAsync("End OrderPaymentLink Troubleshoot", "");
        }
    }
}