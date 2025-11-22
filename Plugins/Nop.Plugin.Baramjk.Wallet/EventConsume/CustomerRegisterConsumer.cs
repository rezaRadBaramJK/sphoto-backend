using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Events.Customers;
using Nop.Plugin.Baramjk.Framework.Services.Currencies;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Plugins;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Services.Events;

namespace Nop.Plugin.Baramjk.Wallet.EventConsume
{
    public class CustomerRegisterConsumer : IConsumer<CustomerRegisterEvent>
    {
        private readonly ICurrencyTools _currencyTools;
        private readonly IWalletService _walletService;
        private readonly WalletSettings _walletSettings;

        public CustomerRegisterConsumer(WalletSettings walletSettings, IWalletService walletService,
            ICurrencyTools currencyTools)
        {
            _walletSettings = walletSettings;
            _walletService = walletService;
            _currencyTools = currencyTools;
        }

        public async Task HandleEventAsync(CustomerRegisterEvent eventMessage)
        {
            if (_walletSettings.AmountForRegistration > 0 == false)
                return;

            var result = await _currencyTools.ConvertPrimaryToAsync(_walletSettings.AmountForRegistration,
                _walletSettings.DefaultCurrencyCode);
            await _walletService.PerformAsync(new WalletTransactionRequest
            {
                Amount = result.Amount,
                CurrencyCode = _walletService.DefaultCurrencyCode,
                CustomerId = eventMessage.Entity.Id,
                Type = WalletHistoryType.Reward,
                Note = "customer register reward"
            });
            
        }
    }
}