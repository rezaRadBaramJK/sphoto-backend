using System.Threading.Tasks;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Services.Directory;

namespace Nop.Plugin.Baramjk.Wallet.Plugins
{
    public class Troubleshoot : TroubleshootBase
    {
        private readonly ICurrencyService _currencyService;

        public Troubleshoot(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        public override async Task TroubleshootAsync()
        {
            await LogAsync("Start Wallet Troubleshoot", "");
            await TroubleshootSettingAsync(WalletPlugin.GetDefaultSetting);
            await AddLocaleResourceAsync(WalletPlugin.GetLocalizationResources);
            await TroubleshootMigrationAsync();
            await InstallPermissionsAsync(new PermissionProvider());
            await CreateCurrencyAsync();
            await LogAsync("End Wallet Troubleshoot", "");
        }
        
        private async Task CreateCurrencyAsync()
        {
            var goldCurrency = await _currencyService.GetCurrencyByCodeAsync(PublicValues.CoinGoldCurrencyCode);
            if (goldCurrency == null)
            {
                var currency = new Currency
                {
                    Name = "GOLD",
                    Rate = 3,
                    CurrencyCode = PublicValues.CoinGoldCurrencyCode,
                    RoundingType = RoundingType.Rounding001,
                    Published = true
                };
                await _currencyService.InsertCurrencyAsync(currency);
            }

            var silverCurrency = await _currencyService.GetCurrencyByCodeAsync(PublicValues.CoinSilverCurrencyCode);
            if (silverCurrency == null)
            {
                var currency = new Currency
                {
                    Name = "SILVER",
                    Rate = 5,
                    CurrencyCode = PublicValues.CoinSilverCurrencyCode,
                    RoundingType = RoundingType.Rounding001,
                    Published = true
                };
                await _currencyService.InsertCurrencyAsync(currency);
            }
        }
    }
}