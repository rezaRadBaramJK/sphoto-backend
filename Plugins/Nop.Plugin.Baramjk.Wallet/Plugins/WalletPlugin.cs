using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Nop.Plugins;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Baramjk.Wallet.Plugins
{
    public partial class WalletPlugin : BaramjkPaymentMethodPlugin, IAdminMenuPlugin
    {
        private readonly ISettingService _settingService;
        private readonly WalletProcessPaymentService _walletProcessPaymentService;
        private readonly ILocalizationService _localizationService;

        public WalletPlugin(ISettingService settingService,
            WalletProcessPaymentService walletProcessPaymentService, 
            ILocalizationService localizationService)
        {
            _settingService = settingService;
            _walletProcessPaymentService = walletProcessPaymentService;
            _localizationService = localizationService;
        }

        private static SiteMapNode _pluginSiteMapNode;

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (!await AuthorizeAsync(PermissionProvider.WalletManagementRecord))
                return;

            if (_pluginSiteMapNode == null)
            {
                var nodes = new[]
                {
                    CreateSiteMapNode("Package", "List", "Packages", $"{SystemName}_Wallet_Packages"),
                    CreateSiteMapNode("WalletWithdraw", "Index", "Withdraw"),
                    CreateSiteMapNode("HistoryAdmin", "List", "Histories")
                };
                _pluginSiteMapNode = CreatePluginSiteMapNode(FriendlyName, nodes);
            }

            rootNode.AddToBaramjkPluginsMenu(_pluginSiteMapNode);
        }

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(GetDefaultSetting);
            await PermissionService.InstallPermissionsAsync(new PermissionProvider());
            await _localizationService.AddLocaleResourceAsync(GetLocalizationResources);
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<WalletSettings>();
            await _localizationService.DeleteLocaleResourcesAsync("Nop.Plugin.Baramjk.Wallet");
            await base.UninstallAsync();
        }

        public static WalletSettings GetDefaultSetting => new()
        {
            DefaultCurrencyCode = "KWD",
            PublicViewComponentName = "PaymentInfo",
            SkipPaymentInfo = true
            
        };

        public static Dictionary<string, string> GetLocalizationResources => new()
        {
            {"Nop.Plugin.Baramjk.Wallet", "Wallet"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.IsEnableChargeWallet", "Enable Charge Wallet"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.CurrentAmount", "Current Amount"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.AmountToUpdate", "Amount To Update"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.HistoryType", "History Type"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.AddAmountToUpdateErrorMessage", "Please add amount to update and try again."},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories", "Histories"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.CurrencyCode", "Currency Code"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.CurrencyName", "Currency Name"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.Amount", "Amount"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.Type", "Type"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.Types", "Types"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.OriginatedEntityId", "Originated Entity Id"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.ExpirationDateTime", "Expiration Date"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.CreateDateTime", "Create Date"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.Note", "Note"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.Currencies", "Currencies"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.HistoryNumber", "History #"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.Redeemed", "Redeemed"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.RedeemedForEntityId", "Redeemed For Entity Id"},
            //Package
            {"Nop.Plugin.Baramjk.Wallet.Admin.Packages", "Packages"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Packages.Name", "Name"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Packages.Amount", "Amount"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Packages.CurrencyCode", "CurrencyCode"},
            {"Nop.Plugin.Baramjk.Wallet.Admin.Packages.AddPackageItem", "Add new amount"},
            
        };
    }
}