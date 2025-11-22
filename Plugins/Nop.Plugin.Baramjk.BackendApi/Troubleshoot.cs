using System.Threading.Tasks;
using Nop.Plugin.Baramjk.BackendApi.Services.Security;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;

namespace Nop.Plugin.Baramjk.BackendApi
{
    public class Troubleshoot : TroubleshootBase
    {
        public override async Task TroubleshootAsync()
        {
            await LogAsync("Start Backend Api Troubleshoot", "");
            await InstallPermissionsAsync(new WebApiBackendPermissionProvider());
            await LogAsync("End Backend Api Troubleshoot", "");
        }
    }
}