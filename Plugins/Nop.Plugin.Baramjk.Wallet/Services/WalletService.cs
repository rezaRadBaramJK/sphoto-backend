// using System;
// using System.Collections.Generic;
// using System.Globalization;
// using System.Linq;
// using System.Threading.Tasks;
// using Nop.Core;
// using Nop.Core.Domain.Directory;
// using Nop.Data;
// using Nop.Plugin.Baramjk.Framework.Exceptions;
// using Nop.Plugin.Baramjk.Framework.Services.Wallets;
// using Nop.Plugin.Baramjk.Wallet.Domain;
// using Nop.Plugin.Baramjk.Wallet.Plugins;
// using Nop.Plugin.Baramjk.Wallet.Services.Models;
// using Nop.Services.Catalog;
// using Nop.Services.Directory;
// using Nop.Services.Localization;
//
// namespace Nop.Plugin.Baramjk.Wallet.Services
// {
//     public interface IWalletWithdrawCheck
//     {
//         Task<bool> CanWithdrawalAsync(int customerId, string currencyCode, decimal amount);
//     }
//
//     
//     
//
//     public class WalletService : IWalletService, IWalletWithdrawCheck
//     {
//         private readonly ICurrencyService _currencyService;
//         private readonly CurrencySettings _currencySettings;
//         private readonly IPriceFormatter _priceFormatter;
//         private readonly IRepository<WalletHistory> _walletHistoryRepository;
//         private readonly IRepository<Domain.Wallet> _walletRepository;
//         private readonly IWorkContext _workContext;
//         private readonly WalletSettings _walletSettings;
//         private readonly ILocalizationService _localizationService;
//         private readonly WalletHistoryService _walletHistoryService;
//
//         public WalletService(
//             IRepository<Domain.Wallet> walletRepository,
//             IRepository<WalletHistory> walletHistoryRepository,
//             ICurrencyService currencyService,
//             IWorkContext workContext,
//             CurrencySettings currencySettings,
//             IPriceFormatter priceFormatter,
//             WalletSettings walletSettings,
//             ILocalizationService localizationService,
//             WalletHistoryService walletHistoryService)
//         {
//             _walletRepository = walletRepository;
//             _walletHistoryRepository = walletHistoryRepository;
//             _currencyService = currencyService;
//             _workContext = workContext;
//             _currencySettings = currencySettings;
//             _priceFormatter = priceFormatter;
//             _walletSettings = walletSettings;
//             _localizationService = localizationService;
//             _walletHistoryService = walletHistoryService;
//         }
//
//         public async Task<decimal> GetAvailableAmountAsync(int customerId, string currencyCode)
//         {
//             var wallet = await GetOrCreateWalletByCurrencyCodesAsync(customerId, currencyCode);
//             return await GetAvailableAmount(wallet);
//         }
//
//         public async Task<List<WalletModel>> GetCustomerWalletsAsync(int customerId)
//         {
//             var allCurrencies = await _currencyService.GetAllCurrenciesAsync();
//             var walletModels = new List<WalletModel>();
//             var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
//             var primaryCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
//
//             foreach (var currency in allCurrencies)
//             {
//                 var wallet = await GetWalletAsync(customerId, currency.CurrencyCode);
//                 var walletHistories = _walletHistoryRepository.Table
//                     .Where(item => item.CustomerWalletId == wallet.Id)
//                     .ToList();
//
//                 var totalAmountUsed = walletHistories
//                     .Where(item => item.WalletHistoryType == WalletHistoryType.Spend)
//                     .Sum(item => item.Amount);
//
//                 var extra = walletHistories
//                     .Where(item => item.WalletHistoryType == WalletHistoryType.Extra)
//                     .Sum(item => item.Amount);
//
//                 var availableAmount = wallet.Amount - wallet.LockAmount;
//                 var remainingCredit = availableAmount - extra;
//
//                 var walletCurrency = await _currencyService.GetCurrencyByIdAsync(wallet.CurrencyId);
//                 var inPrimary =
//                     await _currencyService.ConvertCurrencyAsync(availableAmount, walletCurrency, primaryCurrency);
//                 //await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(availableAmount, primaryCurrency);
//                 var inCurrent =
//                     await _currencyService.ConvertCurrencyAsync(availableAmount, walletCurrency, currentCurrency);
//                 // await _priceFormatter.FormatPriceAsync(0);
//
//                 var customerWallet = new WalletModel
//                 {
//                     Id = wallet.Id,
//                     CustomerId = wallet.CustomerId,
//                     CurrencyCode = wallet.CurrencyCode,
//                     CurrencyId = wallet.CurrencyId,
//                     Amount = wallet.Amount,
//                     LockAmount = wallet.LockAmount,
//                     IsLocked = wallet.IsLocked,
//                     AvailableAmount = availableAmount,
//                     CurrencyName = await _localizationService.GetLocalizedAsync(currency, x => x.Name),
//                     CurrencyRate = currency.Rate,
//                     TotalAmountUsed = totalAmountUsed,
//                     RemainingCredit = remainingCredit,
//                     ExtraCredit = extra,
//                     TotalRemaining = availableAmount,
//                     TotalRemainingInPrimary = inPrimary,
//                     TotalRemainingInCurrent = inCurrent,
//                     TotalRemainingDisplay = GetCurrencyString(availableAmount, true, walletCurrency),
//                     TotalRemainingInPrimaryDisplay = GetCurrencyString(inPrimary, true, primaryCurrency),
//                     TotalRemainingInCurrentDisplay = GetCurrencyString(inCurrent, true, currentCurrency)
//                 };
//                 walletModels.Add(customerWallet);
//             }
//
//             return walletModels;
//         }
//
//         public async Task<bool> LockAsync(int customerId, string currencyCode, decimal amount)
//         {
//             var wallet = await GetWalletAsync(customerId, currencyCode);
//             if (amount >= await GetAvailableAmount(wallet))
//                 return false;
//
//             wallet.LockAmount += amount;
//             await UpdateWallet(wallet, WalletHistoryType.Locking, amount);
//             return true;
//         }
//
//         public async Task<bool> UnLockAsync(int customerId, string currencyCode, decimal amount)
//         {
//             var wallet = await GetWalletAsync(customerId, currencyCode);
//
//             //First try to reduce the amount from reward wallet histories(to make sure the wallet histories that are being expired sooner, get used sooner!)
//             var remainingAmount = await CalculateRewardWalletHistories(wallet, WalletHistoryType.UnLocking, amount);
//
//             wallet.LockAmount -= remainingAmount;
//             await UpdateWallet(wallet, WalletHistoryType.UnLocking, amount);
//             return true;
//         }
//
//         public async Task<bool> CanWithdrawalAsync(int customerId, string currencyCode, decimal amount)
//         {
//             var wallet = await GetWalletAsync(customerId, currencyCode);
//             if (amount >= await GetAvailableAmount(wallet))
//                 return false;
//             return true;
//         }
//
//         public async Task<bool> WithdrawalAsync(int customerId, string currencyCode, decimal amount)
//         {
//             var wallet = await GetWalletAsync(customerId, currencyCode);
//             if (amount >= await GetAvailableAmount(wallet))
//                 return false;
//
//             //First try to reduce the amount from reward wallet histories(to make sure the wallet histories that are being expired sooner, get used sooner!)
//             var remainingAmount = await CalculateRewardWalletHistories(wallet, WalletHistoryType.Withdrawal, amount);
//
//             wallet.Amount -= remainingAmount;
//
//             await UpdateWallet(wallet, WalletHistoryType.Withdrawal, amount);
//             return true;
//         }
//
//         public async Task<bool> TransferAsync(int fromCustomerId, int toCustomerId, string currencyCode, decimal amount)
//         {
//             var sourceWallet = await GetWalletAsync(fromCustomerId, currencyCode);
//             if (sourceWallet == null)
//                 throw new BadRequestBusinessException("Source Wallet Not Found");
//
//             if (await GetAvailableAmount(sourceWallet) < amount)
//                 throw new BadRequestBusinessException("The account balance is insufficient");
//
//             var disWallet = await GetWalletAsync(toCustomerId, currencyCode);
//             if (disWallet == null)
//                 throw new BadRequestBusinessException("Distance Wallet Not Found");
//
//             sourceWallet.Amount -= amount;
//             disWallet.Amount += amount;
//
//
//             //First try to reduce the amount from reward wallet histories(to make sure the wallet histories that are being expired sooner, get used sooner!)
//             //No should not be done for increasing!
//             var remainingAmount =
//                 await CalculateRewardWalletHistories(sourceWallet, WalletHistoryType.TransferSend, amount);
//             await UpdateWallet(sourceWallet, WalletHistoryType.TransferSend, remainingAmount);
//
//
//             await UpdateWallet(disWallet, WalletHistoryType.TransferReceive, amount);
//
//             return true;
//         }
//
//         public async Task<bool> ChargeAsync(int customerId, string currencyCode, decimal amount)
//         {
//             return await ChargeAsync(customerId, currencyCode, amount, WalletHistoryType.Charge);
//         }
//
//         public async Task<bool> ChargeAsync(int customerId, string currencyCode, decimal amount, WalletHistoryType type)
//         {
//             var wallet = await GetWalletAsync(customerId, currencyCode);
//             if (wallet == null)
//                 throw new BadRequestBusinessException("wallet not found");
//
//             wallet.Amount += amount;
//             await UpdateWallet(wallet, type, amount);
//             return true;
//         }
//
//         public async Task<List<CustomerWallet>> GetWalletsAsync(int customerId)
//         {
//             var allCurrencies = await _currencyService.GetAllCurrenciesAsync();
//             var objects = new List<CustomerWallet>();
//             foreach (var currency in allCurrencies)
//             {
//                 var wallet = await GetWalletAsync(customerId, currency.CurrencyCode);
//                 var customerWallet = new CustomerWallet
//                 {
//                     Wallet = wallet,
//                     Currency = currency
//                 };
//                 objects.Add(customerWallet);
//             }
//
//             return objects;
//         }
//
//         public async Task<Domain.Wallet> GetWalletAsync(int customerId, string currencyCode)
//         {
//             var wallet = await GetOrCreateWalletByCurrencyCodesAsync(customerId, currencyCode);
//             return wallet;
//         }
//
//         public string DefaultCurrencyCode => _walletSettings.DefaultCurrencyCode;
//
//         public bool IsForceUseWalletCredit()
//         {
//             return _walletSettings.ForceUseWalletCredit;
//         }
//
//         public Task<bool> WithdrawalAsync(int customerId, string currencyCode, decimal amount, int orderId = 0)
//         {
//             return _walletHistoryService.WithdrawalAsync(customerId, currencyCode, amount, orderId);
//         }
//
//         private async Task<decimal> GetAvailableAmount(Domain.Wallet wallet)
//         {
//             //Remove the expired reward wallet histories!
//             wallet = await RemoveExpiredHistoriesBalanceFromWallet(wallet);
//             return (wallet.Amount - wallet.LockAmount);
//         }
//
//         private async Task<Domain.Wallet> GetOrCreateWalletByCurrencyCodesAsync(int customerId, string currencyCode)
//         {
//             var wallet = await _walletRepository.Table
//                 .FirstOrDefaultAsync(item => item.CustomerId == customerId && item.CurrencyCode == currencyCode);
//
//             if (wallet != null)
//                 return wallet;
//
//             wallet = await AddWallet(customerId, currencyCode);
//             return wallet;
//         }
//
//         private async Task<Domain.Wallet> AddWallet(int customerId, string currencyCode)
//         {
//             var currency = await _currencyService.GetCurrencyByCodeAsync(currencyCode);
//             if (currency == null)
//                 throw new NotFoundBusinessException("Currency not found");
//
//             var wallet = new Domain.Wallet
//             {
//                 CustomerId = customerId,
//                 CurrencyCode = currencyCode,
//                 CurrencyId = currency.Id
//             };
//
//             await _walletRepository.InsertAsync(wallet);
//             return wallet;
//         }
//
//         private async Task UpdateWallet(Domain.Wallet wallet, WalletHistoryType type, decimal amount)
//         {
//             DateTime? expirationDateTime = null;
//             var createDateTime = DateTime.UtcNow;
//             if (_walletSettings.RewardExpirationDays > 0 && type == WalletHistoryType.Reward)
//             {
//                 expirationDateTime = createDateTime.AddDays(_walletSettings.RewardExpirationDays);
//             }
//
//             var walletHistory = new WalletHistory
//             {
//                 Amount = amount,
//                 CustomerWalletId = wallet.Id,
//                 WalletHistoryType = type,
//                 CreateDateTime = createDateTime,
//                 ExpirationDateTime = expirationDateTime
//             };
//
//             await _walletRepository.UpdateAsync(wallet);
//             await _walletHistoryRepository.InsertAsync(walletHistory);
//         }
//
//         private async Task<Domain.Wallet> RemoveExpiredHistoriesBalanceFromWallet(Domain.Wallet wallet)
//         {
//             var walletHistories = await _walletHistoryRepository.Table.Where(x => x.CustomerWalletId == wallet.Id)
//                 .Where(x => x.ExpirationDateTime != null && x.ExpirationDateTime < DateTime.UtcNow)
//                 .Where(x => !x.Redeemed)
//                 .Where(x => x.WalletHistoryType == WalletHistoryType.Reward)
//                 .OrderBy(x => x.ExpirationDateTime)
//                 .ToListAsync();
//
//             foreach (var walletHistory in walletHistories)
//             {
//                 wallet.Amount -= walletHistory.Amount;
//                 walletHistory.Redeemed = true;
//                 await _walletHistoryRepository.UpdateAsync(walletHistory);
//             }
//
//             await _walletRepository.UpdateAsync(wallet);
//
//             return wallet;
//         }
//
//         //This method will only be used when reducing wallet amount
//         private async Task<decimal> CalculateRewardWalletHistories(Domain.Wallet wallet, WalletHistoryType type,
//             decimal amount)
//         {
//             var remainingAmount = amount;
//             var walletHistories = await _walletHistoryRepository.Table.Where(x => x.CustomerWalletId == wallet.Id)
//                 .Where(x => x.ExpirationDateTime != null && x.ExpirationDateTime > DateTime.UtcNow)
//                 .Where(x => !x.Redeemed)
//                 .Where(x => x.WalletHistoryType == WalletHistoryType.Reward)
//                 .OrderBy(x => x.ExpirationDateTime)
//                 .ToListAsync();
//             switch (type)
//             {
//                 case WalletHistoryType.Spend:
//                 case WalletHistoryType.Locking:
//                 case WalletHistoryType.Charge:
//                 case WalletHistoryType.Extra:
//                 case WalletHistoryType.Deposit:
//                 case WalletHistoryType.TransferReceive:
//                 case WalletHistoryType.FreeAd:
//                 case WalletHistoryType.Reward:
//                     break;
//                 case WalletHistoryType.UnLocking:
//                 case WalletHistoryType.Withdrawal:
//                 case WalletHistoryType.TransferSend:
//                     var updatedWalletHitories = new List<WalletHistory>();
//                     foreach (var walletHistory in walletHistories)
//                     {
//                         var transactionAmount = Math.Min(remainingAmount, walletHistory.Amount);
//                         walletHistory.Redeemed = true;
//                         updatedWalletHitories.Add(walletHistory);
//                         remainingAmount -= transactionAmount;
//                         wallet.Amount -= walletHistory.Amount;
//                         if (remainingAmount <= 0)
//                         {
//                             break;
//                         }
//                     }
//
//                     await _walletHistoryRepository.UpdateAsync(updatedWalletHitories);
//
//                     break;
//             }
//
//             return remainingAmount;
//         }
//
//         protected virtual string GetCurrencyString(decimal amount,
//             bool showCurrency, Currency targetCurrency)
//         {
//             if (targetCurrency == null)
//                 throw new ArgumentNullException(nameof(targetCurrency));
//
//             string result;
//             if (!string.IsNullOrEmpty(targetCurrency.CustomFormatting))
//                 //custom formatting specified by a store owner
//             {
//                 result = amount.ToString(targetCurrency.CustomFormatting);
//             }
//             else
//             {
//                 if (!string.IsNullOrEmpty(targetCurrency.DisplayLocale))
//                     //default behavior
//                 {
//                     result = amount.ToString("C", new CultureInfo(targetCurrency.DisplayLocale));
//                 }
//                 else
//                 {
//                     //not possible because "DisplayLocale" should be always specified
//                     //but anyway let's just handle this behavior
//                     result = $"{amount:N} ({targetCurrency.CurrencyCode})";
//                     return result;
//                 }
//             }
//
//             //display currency code?
//             if (showCurrency && _currencySettings.DisplayCurrencyLabel)
//                 result = $"{result} ({targetCurrency.CurrencyCode})";
//             return result;
//         }
//     }
// }