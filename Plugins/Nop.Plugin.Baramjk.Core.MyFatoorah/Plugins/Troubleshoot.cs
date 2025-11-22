using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins
{
    public class Troubleshoot : TroubleshootBase
    {
        private readonly SupplierService _supplierService;

        public Troubleshoot(SupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        public override async Task TroubleshootAsync()
        {
            await LogAsync("Start MyFatoorah core Troubleshoot", "");
            await TroubleshootSettingAsync(MyFatoorahPlugin.GetDefaultSetting);
            await TroubleshootMigrationAsync();
            await InstallPermissionsAsync(new PermissionProvider());
            await AddLocaleResourceAsync(MyFatoorahPlugin.LocalizationResources);
            await _supplierService.SubmitAsync();
            
            await LogAsync("End MyFatoorah core Troubleshoot", "");
        }
    }
}