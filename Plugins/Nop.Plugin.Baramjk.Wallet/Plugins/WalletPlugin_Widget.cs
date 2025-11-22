using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Services.Cms;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Baramjk.Wallet.Plugins
{
    public partial class WalletPlugin : IWidgetPlugin
    {
        public bool HideInWidgetList => false;

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.AccountNavigationAfter,
                AdminWidgetZones.CustomerDetailsBlock
            });
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "WidgetsWallet";
        }
    }
}