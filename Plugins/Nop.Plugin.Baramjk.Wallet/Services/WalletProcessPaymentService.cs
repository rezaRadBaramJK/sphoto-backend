using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Plugins;
using Nop.Services.Directory;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.Wallet.Services
{
    public class WalletProcessPaymentService
    {
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWalletService _walletService;
        private readonly WalletSettings _walletSettings;
        private readonly IWorkContext _workContext;

        public WalletProcessPaymentService(IWorkContext workContext, IWalletService walletService,
            ICurrencyService currencyService, WalletSettings walletSettings, CurrencySettings currencySettings)
        {
            _workContext = workContext;
            _walletService = walletService;
            _currencyService = currencyService;
            _walletSettings = walletSettings;
            _currencySettings = currencySettings;
        }

        public async Task<string> PayAsync(decimal total)
        {
            var currencyCode = _walletSettings.DefaultCurrencyCode;
            var currentCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;

            total = await CalcTotalTWithdrawalAsync(total, currencyCode);

            var withdrawalResult = await _walletService.PerformAsync(new WalletTransactionRequest
            {
                CustomerId = currentCustomerId,
                CurrencyCode = currencyCode,
                Amount = total,
                Type = WalletHistoryType.Withdrawal,
                Note = "wallet pay"
            });
            if (withdrawalResult == false)
                return "you need to charge your wallet";

            return null;
        }

        private async Task<decimal> CalcTotalTWithdrawalAsync(decimal total, string currencyCode)
        {
            var currencyWithdrawal = await _currencyService.GetCurrencyByCodeAsync(currencyCode);
            var primaryCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            if (primaryCurrency.Id == currencyWithdrawal.Id)
                return total;

            total = await _currencyService.ConvertCurrencyAsync(total, primaryCurrency, currencyWithdrawal);
            return total;
        }
    }
}