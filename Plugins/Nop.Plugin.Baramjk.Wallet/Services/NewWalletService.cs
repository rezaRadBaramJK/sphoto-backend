using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
using iTextSharp.text;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Domain;
using Nop.Plugin.Baramjk.Wallet.Models.Service;
using Nop.Plugin.Baramjk.Wallet.Plugins;
using Nop.Plugin.Baramjk.Wallet.Services.Models;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.Wallet.Services
{
    public interface IInternalWalletService
    {
        Task<Framework.Services.Wallets.Domains.Wallet> RemoveExpiredHistoriesBalanceFromWallet(
            Framework.Services.Wallets.Domains.Wallet wallet);

        Task<List<CustomerWallet>> GetCustomerWalletsAsync(int customerId);

        Task<bool> RevertWalletHistoryForCancelledOrder(int customerId, string currencyCode, int originatedEntityId,
            int redeemedForEntityId);

        Task<Framework.Services.Wallets.Domains.Wallet> GetOrCreateWalletByCurrencyCodesAsync(int customerId,
            string currencyCode);

        Task<Framework.Services.Wallets.Domains.Wallet> GetWalletAsync(int customerId, string currencyCode);

        Task AddWalletHistory(Framework.Services.Wallets.Domains.Wallet wallet, WalletHistoryType type, decimal amount,
            int originatedEntityId,
            int redeemedForEntityId, object extraData, Guid? txid, bool isRevert, string note,
            DateTime expireDate = default);

        Task<IPagedList<SearchHistoryServiceResult>> SearchHistories(
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            List<int> typeIds = null,
            List<int> currencyIds = null,
            string email = "",
            string userName = "",
            string note = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        Task<CustomerWalletInfoModel> GetCustomerWalletInfoAsync(int customerId, string currencyCode);
    }

    public enum TransactionType
    {
        Deposit,
        Withdraw
    }

    public class WalletService : IWalletService, IInternalWalletService
    {
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IRepository<WalletHistory> _walletHistoryRepository;
        private readonly IRepository<Framework.Services.Wallets.Domains.Wallet> _walletRepository;
        private readonly IWorkContext _workContext;
        private readonly WalletSettings _walletSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Currency> _currencyRepository;

        public WalletService(
            IRepository<Framework.Services.Wallets.Domains.Wallet> walletRepository,
            IRepository<WalletHistory> walletHistoryRepository,
            ICurrencyService currencyService,
            IWorkContext workContext,
            CurrencySettings currencySettings,
            IPriceFormatter priceFormatter,
            WalletSettings walletSettings,
            ILocalizationService localizationService,
            ILogger logger, IRepository<Customer> customerRepository,
            IRepository<Currency> currencyRepository)
        {
            _walletRepository = walletRepository;
            _walletHistoryRepository = walletHistoryRepository;
            _currencyService = currencyService;
            _workContext = workContext;
            _currencySettings = currencySettings;
            _priceFormatter = priceFormatter;
            _walletSettings = walletSettings;
            _localizationService = localizationService;
            _logger = logger;
            _customerRepository = customerRepository;
            _currencyRepository = currencyRepository;
        }

        public async Task<decimal> GetAvailableAmountAsync(int customerId, string currencyCode)
        {
            var wallet = await GetOrCreateWalletByCurrencyCodesAsync(customerId, currencyCode);
            return await GetAvailableAmount(wallet);
        }

        public async Task<CustomerWalletInfoModel> GetCustomerWalletInfoAsync(int customerId, string currencyCode)
        {
            var wallet = await GetOrCreateWalletByCurrencyCodesAsync(customerId, currencyCode);

            var totalCashBackSpend = await _walletHistoryRepository.Table
                .Where(x => x.CustomerWalletId == wallet.Id && GetWithdrawalTypes().Contains(x.WalletHistoryType))
                .SumAsync(x => x.Amount);

            var availableAmount = await GetAvailableAmount(wallet);


            //reward earned = remaining amount  + expended amount
            CustomerWalletInfoModel model = new()
            {
                AvailableAmount = availableAmount,
                TotalCashBackReward = availableAmount + totalCashBackSpend,
                TotalCashBackSpend = totalCashBackSpend,
            };
            return model;
        }

        // private async Task<decimal> GetAvailableAmount(Framework.Services.Wallets.Domains.Wallet wallet)
        // {
        //     //Remove the expired reward wallet histories!
        //     wallet = await RemoveExpiredHistoriesBalanceFromWallet(wallet);
        //     return (wallet.Amount - wallet.LockAmount);
        // }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wallet">customer wallet</param>
        /// <param name="walletHistory">This history type is reward</param>
        /// <returns></returns>
        private async Task<decimal> GetRewardLeftBalance(Framework.Services.Wallets.Domains.Wallet wallet,
            WalletHistory walletHistory)
        {
            var histories = await GetWalletHistories(wallet);

            //get all rewards are expire time between walletHistory created time and expire time(or rewards have not expire time)   
            var activeBeforeAndIncludingRewards = histories
                .Where(x => x.WalletHistoryType == WalletHistoryType.Reward)
                .Where(x => x.Id == walletHistory.Id || (x.ExpirationDateTime >= walletHistory.CreateDateTime &&
                                                         (x.ExpirationDateTime == default ||
                                                          x.ExpirationDateTime <= walletHistory.ExpirationDateTime)))
                .ToList();
            
            //dictionary of all rewards expiry time (if null=> created time) and amount
            //We need a pre-withdrawal rewards
            var beforeRewards = new Dictionary<DateTime, decimal>();
            foreach (var activeBeforeReward in activeBeforeAndIncludingRewards)
            {
                var key = activeBeforeReward.ExpirationDateTime ?? activeBeforeReward.CreateDateTime;
                beforeRewards[key] = activeBeforeReward.Amount;
            }

            //get all withdrawals if created time between created time and expire time of wallet history 
            //All withdrawals between the time the reward was created and its expiration time
            var withdrawsAfterRewardWhileRewardNotExpired = histories
                .Where(x => GetWithdrawalTypes().Contains(x.WalletHistoryType))
                .Where(x => x.CreateDateTime > walletHistory.CreateDateTime &&
                            x.CreateDateTime <= walletHistory.ExpirationDateTime)
                .ToList();

            //Subtract withdrawals from allowed rewards (not expired at the time of withdrawal)
            foreach (var withdrawal in
                     withdrawsAfterRewardWhileRewardNotExpired.OrderBy(x => x.CreateDateTime))
            {
                //DateTime of all rewards that a withdrawal can use
                //These rewards must be redeemed just after the withdrawal time.
                var effectiveRewards = beforeRewards.Keys
                    .Where(x => x > withdrawal.CreateDateTime)
                    .OrderBy(x => x)
                    .ToList();
                
                //Calculation related to withdrawal of rewards
                var remainedWithdrawAmount = withdrawal.Amount;
                foreach (var effectiveReward in effectiveRewards)
                {
                    
                    if (beforeRewards[effectiveReward] > Decimal.Zero &&
                        remainedWithdrawAmount > Decimal.Zero)
                    {
                        var withdrawAmount = Math.Min(remainedWithdrawAmount,
                            beforeRewards[effectiveReward]);
                        //update beforeRewards[effectiveReward] amount value
                        beforeRewards[effectiveReward] -= withdrawAmount;
                    }
                }
            }

            //update wallet history amount value and return it
            var rewardKey = walletHistory.ExpirationDateTime ?? walletHistory.CreateDateTime;
            
            //check wallet history is expired
            if (walletHistory.ExpirationDateTime != default &&
                walletHistory.ExpirationDateTime < DateTime.Now ) //expired
            {
                return walletHistory.Amount -
                       beforeRewards[rewardKey]; // used amount in active duration is reward balance
            }

            return walletHistory.Amount;
            // return beforeRewards[rewardKey]; // left balance
        }

        private async Task<decimal> GetAvailableAmount(Framework.Services.Wallets.Domains.Wallet wallet)
        {
            //Remove the expired reward wallet histories!
            // wallet = await RemoveExpiredHistoriesBalanceFromWallet(wallet);
            var histories = await GetWalletHistories(wallet);
            var balance = decimal.Zero;
            var locked = decimal.Zero;
            foreach (var walletHistory in histories)
            {
                switch (walletHistory.WalletHistoryType)
                {
                    case WalletHistoryType.Spend:
                        balance -= walletHistory.Amount;
                        break;
                    case WalletHistoryType.Locking:
                        balance -= walletHistory.Amount;
                        locked += walletHistory.Amount;
                        break;
                    case WalletHistoryType.UnLocking:
                        balance += walletHistory.Amount;
                        locked -= walletHistory.Amount;
                        break;
                    case WalletHistoryType.Charge:
                        balance += walletHistory.Amount;
                        break;
                    case WalletHistoryType.Extra: // what is this?
                        break;
                    case WalletHistoryType.Deposit:
                        balance += walletHistory.Amount;
                        break;
                    case WalletHistoryType.Withdrawal:
                        balance -= Math.Abs(walletHistory.Amount);
                        break;
                    case WalletHistoryType.TransferSend:
                        balance -= walletHistory.Amount;
                        break;
                    case WalletHistoryType.TransferReceive:
                        balance += walletHistory.Amount;
                        break;
                    case WalletHistoryType.FreeAd: // what is this?
                        break;
                    case WalletHistoryType.Reward:
                        balance += await GetRewardLeftBalance(wallet, walletHistory);

                        break;
                    // case WalletHistoryType.Reward:
                    //     var activeBeforeRewardsSums = histories
                    //         .Where(x => x.WalletHistoryType == WalletHistoryType.Reward)
                    //         .Where(x => x.ExpirationDateTime >= walletHistory.CreateDateTime &&
                    //                     x.ExpirationDateTime < walletHistory.ExpirationDateTime)
                    //         .Sum(x => x.Amount);
                    //     var withdrawsAfterRewardWhileRewardNotExpired = histories
                    //         .Where(x => GetWithdrawalTypes().Contains(x.WalletHistoryType))
                    //         .Where(x => x.CreateDateTime > walletHistory.CreateDateTime &&
                    //                     x.CreateDateTime < walletHistory.ExpirationDateTime)
                    //         .Sum(x => x.Amount);
                    //     var amountRewardUsed =
                    //         Math.Min(withdrawsAfterRewardWhileRewardNotExpired - activeBeforeRewardsSums,
                    //             walletHistory.Amount);
                    //     balance += walletHistory.Amount - amountRewardUsed;
                    //     break;
                    case WalletHistoryType.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return (balance - locked);
        }

        private async Task<List<WalletHistory>> GetWalletHistories(Framework.Services.Wallets.Domains.Wallet wallet)
        {
            var histories = await _walletHistoryRepository.Table.Where(x => x.CustomerWalletId == wallet.Id)
                .OrderBy(x => x.CreateDateTime)
                .ToListAsync();
            return histories;
        }

        public async Task<Framework.Services.Wallets.Domains.Wallet> GetOrCreateWalletByCurrencyCodesAsync(
            int customerId,
            string currencyCode)
        {
            var wallet = await _walletRepository.Table
                .FirstOrDefaultAsync(item => item.CustomerId == customerId && item.CurrencyCode == currencyCode);

            if (wallet != null)
                return wallet;

            wallet = await AddWallet(customerId, currencyCode);
            return wallet;
        }


        private async Task<Framework.Services.Wallets.Domains.Wallet> AddWallet(int customerId, string currencyCode)
        {
            var currency = await _currencyService.GetCurrencyByCodeAsync(currencyCode);
            if (currency == null)
                throw new NotFoundBusinessException("Currency not found");

            var wallet = new Framework.Services.Wallets.Domains.Wallet
            {
                CustomerId = customerId,
                CurrencyCode = currencyCode,
                CurrencyId = currency.Id
            };

            await _walletRepository.InsertAsync(wallet);
            return wallet;
        }

        private TransactionType GetTransactionType(WalletHistoryType type)
        {
            if (GetDepositTypes().Contains(type))
            {
                return TransactionType.Deposit;
            }

            if (GetWithdrawalTypes().Contains(type))
            {
                return TransactionType.Withdraw;
            }

            throw new ArgumentOutOfRangeBusinessException(
                $"wallet history type:{type} is not either deposit or withdraw");
        }

        public async Task<bool> CanPerformAsync(WalletTransactionRequest request)
        {
            //Checking if wallet charging is enabled
            if (request.Type == WalletHistoryType.Charge && !_walletSettings.IsEnableChargeWallet)
            {
                throw new Exception("Wallet recharge is disabled.");
            }


            if (string.IsNullOrEmpty(request.Note))
            {
                await _logger.ErrorAsync($"wallet note is null {JsonConvert.SerializeObject(request)}");
                throw new Exception("wallet note is null");
            }

            if (request.Amount < decimal.Zero)
            {
                await _logger.ErrorAsync(
                    $"CanPerformAsync amount can't be negative customerid={request.CustomerId} amount={request.Amount}");
            }

            switch (GetTransactionType(request.Type))
            {
                case TransactionType.Deposit:
                    return await CanDeposit(request.CustomerId, request.CurrencyCode, request.Amount);
                case TransactionType.Withdraw:
                    return await CanWithdraw(request.CustomerId, request.CurrencyCode, request.Amount);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Todo : check with reze
        public async Task<bool> CanRevertAsync(WalletHistory historyToRevert, string comment)
        {
            return false;
            // if (historyToRevert.IsReverted)
            // {
            //     return false;
            // }
            //
            // var revertRequest = await GetRevertTransaction(historyToRevert, comment);
            // return await CanPerformAsync(revertRequest);
        }
        // Todo : check with reze

        public async Task<bool> CanRevertTransferAsync(Guid txid, string comment)
        {
            var transferHistories = await _walletHistoryRepository.Table.Where(x => x.TxId == txid && !x.IsReverted)
                .ToListAsync();
            if (transferHistories.Count != 2)
            {
                return false;
            }

            return (await CanRevertAsync(transferHistories[0], comment)) &&
                   (await CanRevertAsync(transferHistories[1], comment));
        }

        private async Task<WalletTransactionRequest> GetRevertTransaction(WalletHistory historyToRevert, string comment)
        {
            var wallet = await _walletRepository.Table.Where(x => x.Id == historyToRevert.CustomerWalletId)
                .FirstOrDefaultAsync();
            if (wallet == default)
            {
                return default;
            }

            var revertTransaction = new WalletTransactionRequest
            {
                Amount = historyToRevert.Amount,
                CurrencyCode = DefaultCurrencyCode,
                Type = historyToRevert.WalletHistoryType.Negate(),
                CustomerId = wallet.CustomerId,
                ExtraData = $"revert : {historyToRevert.Note} comment: {comment}",
                Note = $"revert : {historyToRevert.Note} comment: {comment}",
                TxId = historyToRevert.TxId,
                IsRevert = true,
            };
            return revertTransaction;
        }

        public async Task<bool> RevertAsync(WalletHistory historyToRevert, string comment)
        {
            if (await CanRevertAsync(historyToRevert, comment))
            {
                var revertRequest = await GetRevertTransaction(historyToRevert, comment);

                var performResult = await PerformAsync(revertRequest);
                if (performResult)
                {
                    historyToRevert.IsReverted = true;
                    await _walletHistoryRepository.UpdateAsync(historyToRevert);
                    return true;
                }
            }


            return false;
        }

        public async Task<bool> RevertTransferAsync(Guid txid, string comment)
        {
            if (await CanRevertTransferAsync(txid, comment))
            {
                var transferHistories = await _walletHistoryRepository.Table.Where(x => x.TxId == txid && !x.IsReverted)
                    .ToListAsync();
                if (transferHistories.Count != 2)
                {
                    return false;
                }

                return (await RevertAsync(transferHistories[0], comment) &&
                        (await RevertAsync(transferHistories[1], comment)));
            }

            return false;
        }

        private async Task<bool> CanWithdraw(int customerId, string currencyCode, decimal amount)
        {
            var wallet = await GetWalletAsync(customerId, currencyCode);
            var walletAmount = await GetAvailableAmount(wallet);
            if (amount > walletAmount)
            {
                await _logger.InformationAsync(
                    $"cant perform CanWithdraw customerId={customerId},currencyCode={currencyCode},amount={amount},walletAmount={walletAmount}");
                return false;
            }

            return true;
        }

        private async Task<bool> CanDeposit(int customerId, string currencyCode, decimal amount)
        {
            // todo : any logic to control deposit such as banned user
            return true;
        }

        private async Task UpdateWallet(Framework.Services.Wallets.Domains.Wallet wallet,
            TransactionType transactionType,
            decimal amount)
        {
            switch (transactionType)
            {
                case TransactionType.Deposit:
                    wallet.Amount += amount;
                    break;
                case TransactionType.Withdraw:
                    wallet.Amount -= amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await _walletRepository.UpdateAsync(wallet);
        }


        public async Task AddWalletHistory(Framework.Services.Wallets.Domains.Wallet wallet, WalletHistoryType type,
            decimal amount,
            int originatedEntityId, int redeemedForEntityId, object extraData, Guid? txid, bool isRevert, string note,
            DateTime expireDate = default)
        {
            if (string.IsNullOrEmpty(note))
            {
                await _logger.ErrorAsync($"wallet note is null AddWalletHistory");
                throw new Exception("wallet note is null in AddWalletHistory");
            }

            DateTime? expirationDateTime = null;
            var createDateTime = DateTime.UtcNow;
            if (_walletSettings.RewardExpirationDays > 0 && type == WalletHistoryType.Reward)
            {
                expirationDateTime = createDateTime.AddDays(_walletSettings.RewardExpirationDays);
            }

            if (expireDate != default)
            {
                await _logger.InformationAsync(
                    $"reward expire date is : {expireDate} legacy expire is : {expirationDateTime}");
                expirationDateTime = expireDate;
            }

            var walletHistory = new WalletHistory
            {
                Amount = amount,
                CustomerWalletId = wallet.Id,
                WalletHistoryType = type,
                CreateDateTime = createDateTime,
                ExpirationDateTime = expirationDateTime,
                OriginatedEntityId = originatedEntityId,
                RedeemedForEntityId = redeemedForEntityId,
                IsReverted = isRevert,
                TxId = txid,
                Note = note
            };
            if (extraData != default)
            {
                walletHistory.Note = JsonConvert.SerializeObject(extraData);
            }


            await _walletHistoryRepository.InsertAsync(walletHistory);
        }

        private async Task UpdateWallet(Framework.Services.Wallets.Domains.Wallet wallet, WalletHistoryType type,
            decimal amount,
            int originatedEntityId, int redeemedForEntityId, object extraData, Guid? txid, bool isRevert, string note,
            DateTime expireDate = default)
        {
            // todo add a db transaction to make it safe
            await UpdateWallet(wallet, GetTransactionType(type), amount);
            await AddWalletHistory(wallet, type, amount, originatedEntityId, redeemedForEntityId, extraData, txid,
                isRevert, note, expireDate);
        }

        public async Task<Framework.Services.Wallets.Domains.Wallet> GetWalletAsync(int customerId, string currencyCode)
        {
            var wallet = await GetOrCreateWalletByCurrencyCodesAsync(customerId, currencyCode);
            return wallet;
        }

        public async Task<bool> PerformAsync(WalletTransactionRequest request)
        {
            if (!await CanPerformAsync(request))
            {
                await _logger.ErrorAsync($"cant perform transaction {JsonConvert.SerializeObject(request)}");
                return false;
            }

            if (string.IsNullOrEmpty(request.Note))
            {
                await _logger.ErrorAsync($"wallet note is null CanPerformAsync");
                throw new Exception("wallet note is null in CanPerformAsync");
            }

            var wallet = await GetWalletAsync(request.CustomerId, request.CurrencyCode);
            try
            {
                await UpdateWallet(wallet, request.Type, request.Amount, request.OriginatedEntityId,
                    request.RedeemedForEntityId, request.ExtraData, request.TxId, request.IsRevert, request.Note,
                    request.ExpireDateTime ?? default);
                return true;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
                return false;
            }
        }

        public async Task<List<WalletModel>> GetCustomerWalletsModelAsync(int customerId)
        {
            var allCurrencies = await _currencyService.GetAllCurrenciesAsync();
            var walletModels = new List<WalletModel>();
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var primaryCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

            foreach (var currency in allCurrencies)
            {
                var wallet = await GetWalletAsync(customerId, currency.CurrencyCode);
                var walletHistories = _walletHistoryRepository.Table
                    .Where(item => item.CustomerWalletId == wallet.Id)
                    .ToList();

                var totalAmountUsed = walletHistories
                    .Where(item => item.WalletHistoryType == WalletHistoryType.Spend)
                    .Sum(item => item.Amount);

                var extra = walletHistories
                    .Where(item => item.WalletHistoryType == WalletHistoryType.Extra)
                    .Sum(item => item.Amount);

                var availableAmount = wallet.Amount - wallet.LockAmount;
                var remainingCredit = availableAmount - extra;

                var walletCurrency = await _currencyService.GetCurrencyByIdAsync(wallet.CurrencyId);
                var inPrimary =
                    await _currencyService.ConvertCurrencyAsync(availableAmount, walletCurrency, primaryCurrency);
                //await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(availableAmount, primaryCurrency);
                var inCurrent =
                    await _currencyService.ConvertCurrencyAsync(availableAmount, walletCurrency, currentCurrency);
                // await _priceFormatter.FormatPriceAsync(0);

                var customerWallet = new WalletModel
                {
                    Id = wallet.Id,
                    CustomerId = wallet.CustomerId,
                    CurrencyCode = wallet.CurrencyCode,
                    CurrencyId = wallet.CurrencyId,
                    Amount = wallet.Amount,
                    LockAmount = wallet.LockAmount,
                    IsLocked = wallet.IsLocked,
                    AvailableAmount = availableAmount,
                    CurrencyName = await _localizationService.GetLocalizedAsync(currency, x => x.Name),
                    CurrencyRate = currency.Rate,
                    TotalAmountUsed = totalAmountUsed,
                    RemainingCredit = remainingCredit,
                    ExtraCredit = extra,
                    TotalRemaining = availableAmount,
                    TotalRemainingInPrimary = inPrimary,
                    TotalRemainingInCurrent = inCurrent,
                    TotalRemainingDisplay = GetCurrencyString(availableAmount, true, walletCurrency),
                    TotalRemainingInPrimaryDisplay = GetCurrencyString(inPrimary, true, primaryCurrency),
                    TotalRemainingInCurrentDisplay = GetCurrencyString(inCurrent, true, currentCurrency)
                };
                walletModels.Add(customerWallet);
            }

            return walletModels;
        }


        public string DefaultCurrencyCode => "KWD";

        public async Task<List<CustomerWallet>> GetCustomerWalletsAsync(int customerId)
        {
            var allCurrencies = await _currencyService.GetAllCurrenciesAsync();
            var wallets = new List<CustomerWallet>();

            foreach (var currency in allCurrencies)
            {
                var wallet = await GetWalletAsync(customerId, currency.CurrencyCode);
                var customerWallet = new CustomerWallet
                {
                    Currency = currency, Wallet = wallet
                };
                wallets.Add(customerWallet);
            }

            return wallets;
        }

        protected virtual string GetCurrencyString(decimal amount,
            bool showCurrency, Currency targetCurrency)
        {
            if (targetCurrency == null)
                throw new ArgumentNullException(nameof(targetCurrency));

            string result;
            if (!string.IsNullOrEmpty(targetCurrency.CustomFormatting))
                //custom formatting specified by a store owner
            {
                result = amount.ToString(targetCurrency.CustomFormatting);
            }
            else
            {
                if (!string.IsNullOrEmpty(targetCurrency.DisplayLocale))
                    //default behavior
                {
                    result = amount.ToString("C", new CultureInfo(targetCurrency.DisplayLocale));
                }
                else
                {
                    //not possible because "DisplayLocale" should be always specified
                    //but anyway let's just handle this behavior
                    result = $"{amount:N} ({targetCurrency.CurrencyCode})";
                    return result;
                }
            }

            //display currency code?
            if (showCurrency && _currencySettings.DisplayCurrencyLabel)
                result = $"{result} ({targetCurrency.CurrencyCode})";
            return result;
        }

        public async Task<bool> CanTransferAsync(WalletTransferTransactionRequest request)
        {
            return (
                (await CanWithdraw(request.FromCustomerId, request.CurrencyCode, request.Amount))
                && (await CanPerformAsync(new WalletTransactionRequest
                {
                    Amount = request.Amount,
                    CurrencyCode = request.CurrencyCode,
                    CustomerId = request.FromCustomerId,
                    ExtraData = request.ExtraData,
                    OriginatedEntityId = request.OriginatedEntityId,
                    RedeemedForEntityId = request.RedeemedForEntityId,
                    Type = WalletHistoryType.TransferSend,
                    Note = request.Note
                }))
                && (await CanDeposit(request.ToCustomerId, request.CurrencyCode, request.Amount))
                && (await CanPerformAsync(new WalletTransactionRequest
                {
                    Amount = request.Amount,
                    CurrencyCode = request.CurrencyCode,
                    CustomerId = request.ToCustomerId,
                    ExtraData = request.ExtraData,
                    OriginatedEntityId = request.OriginatedEntityId,
                    RedeemedForEntityId = request.RedeemedForEntityId,
                    Type = WalletHistoryType.TransferReceive,
                    Note = request.Note
                }))
            );
        }

        public async Task<bool> TransferAsync(WalletTransferTransactionRequest request)
        {
            if (!await CanTransferAsync(request))
            {
                return false;
            }

            try
            {
                var txid = Guid.NewGuid();
                await PerformAsync(new WalletTransactionRequest
                {
                    Amount = request.Amount,
                    CurrencyCode = request.CurrencyCode,
                    CustomerId = request.FromCustomerId,
                    ExtraData = request.ExtraData,
                    OriginatedEntityId = request.OriginatedEntityId,
                    RedeemedForEntityId = request.RedeemedForEntityId,
                    Type = WalletHistoryType.TransferSend,
                    TxId = txid,
                    Note = $"transfer send from: {request.FromCustomerId} to:{request.ToCustomerId} {request.Note} "
                });
                await PerformAsync(new WalletTransactionRequest
                {
                    Amount = request.Amount,
                    CurrencyCode = request.CurrencyCode,
                    CustomerId = request.ToCustomerId,
                    ExtraData = request.ExtraData,
                    OriginatedEntityId = request.OriginatedEntityId,
                    RedeemedForEntityId = request.RedeemedForEntityId,
                    Type = WalletHistoryType.TransferReceive,
                    TxId = txid,
                    Note = $"transfer receive from: {request.FromCustomerId} to:{request.ToCustomerId} {request.Note} "
                });
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
                return false;
            }

            return true;
        }

        public bool IsForceUseWalletCredit()
        {
            return _walletSettings.ForceUseWalletCredit;
        }

        public List<WalletHistoryType> GetDepositTypes()
        {
            return WalletHistoryTypeExtensions.GetDepositTypes();
        }

        public List<WalletHistoryType> GetWithdrawalTypes()
        {
            return WalletHistoryTypeExtensions.GetWithdrawalTypes();
        }

        public async Task<List<WalletHistory>> Search(
            List<WalletHistoryType> types = default, int pageNumber = 1, int pageSize = 10,
            DateTime fromDateTime = default, DateTime toDateTime = default, List<int> customerWalletIds = default)
        {
            var query = _walletHistoryRepository.Table;
            if (fromDateTime != default)
            {
                query = query.Where(x => x.CreateDateTime >= fromDateTime);
            }

            if (toDateTime != default)
            {
                query = query.Where(x => x.CreateDateTime <= toDateTime);
            }

            if (customerWalletIds != default && customerWalletIds.Any())
            {
                query = query.Where(x => customerWalletIds.Contains(x.CustomerWalletId));
            }

            return await query.OrderByDescending(x => x.Id)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageNumber)
                .ToListAsync();
        }

        public async Task<IPagedList<HistoryWithCurrencyCodeServiceResult>> GetHistoriesByCustomerIdAsync(
            int customerId, int pageIndex = 0, int pageSize = 10)
        {
            var query =
                from walletHistory in _walletHistoryRepository.Table
                join wallet in _walletRepository.Table on walletHistory.CustomerWalletId equals wallet.Id
                where wallet.CustomerId == customerId
                orderby walletHistory.CreateDateTime descending
                select new HistoryWithCurrencyCodeServiceResult
                {
                    Id = walletHistory.Id,
                    CurrencyCode = wallet.CurrencyCode,
                    CustomerWalletId = walletHistory.CustomerWalletId,
                    Amount = walletHistory.Amount,
                    WalletHistoryType = walletHistory.WalletHistoryType,
                    CreateDateTime = walletHistory.CreateDateTime,
                    ExpirationDateTime = walletHistory.ExpirationDateTime,
                    OriginatedEntityId = walletHistory.OriginatedEntityId,
                    RedeemedForEntityId = walletHistory.RedeemedForEntityId,
                    Redeemed = walletHistory.Redeemed,
                    Note = walletHistory.Note
                };

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        //This is recreate in porpus of not changing baramjk.framework
        //This method will only be used when reducing wallet amount
        public async Task<decimal> CalculateRewardWalletHistories(Framework.Services.Wallets.Domains.Wallet wallet,
            WalletHistoryType type,
            decimal amount, int orderId)
        {
            var remainingAmount = amount;
            var walletHistories =
                await _walletHistoryRepository.Table
                    .Where(x => x.CustomerWalletId == wallet.Id)
                    .Where(x => x.ExpirationDateTime != null && x.ExpirationDateTime > DateTime.UtcNow)
                    .Where(x => !x.Redeemed)
                    .Where(x => x.WalletHistoryType == WalletHistoryType.Reward)
                    .OrderBy(x => x.ExpirationDateTime)
                    .ToListAsync();
            switch (type)
            {
                case WalletHistoryType.Spend:
                case WalletHistoryType.Locking:
                case WalletHistoryType.Charge:
                case WalletHistoryType.Extra:
                case WalletHistoryType.Deposit:
                case WalletHistoryType.TransferReceive:
                case WalletHistoryType.FreeAd:
                case WalletHistoryType.Reward:
                    break;
                case WalletHistoryType.UnLocking:
                case WalletHistoryType.Withdrawal:
                case WalletHistoryType.TransferSend:
                    var updatedWalletHitories = new List<WalletHistory>();
                    foreach (var walletHistory in walletHistories)
                    {
                        var transactionAmount = Math.Min(remainingAmount, walletHistory.Amount);
                        walletHistory.Redeemed = true;
                        walletHistory.RedeemedForEntityId = orderId;
                        updatedWalletHitories.Add(walletHistory);
                        remainingAmount -= transactionAmount;
                        wallet.Amount -= walletHistory.Amount;
                        if (remainingAmount <= 0)
                        {
                            break;
                        }
                    }

                    await _walletHistoryRepository.UpdateAsync(updatedWalletHitories);

                    break;
            }

            return remainingAmount;
        }

        [Obsolete("causes bugs", true)]
        public async Task<Framework.Services.Wallets.Domains.Wallet> RemoveExpiredHistoriesBalanceFromWallet(
            Framework.Services.Wallets.Domains.Wallet wallet)
        {
            var walletHistories = await _walletHistoryRepository.Table.Where(x => x.CustomerWalletId == wallet.Id)
                .Where(x => x.ExpirationDateTime != null && x.ExpirationDateTime < DateTime.UtcNow)
                .Where(x => !x.Redeemed)
                .Where(x => x.WalletHistoryType == WalletHistoryType.Reward)
                .OrderBy(x => x.ExpirationDateTime)
                .ToListAsync();

            foreach (var walletHistory in walletHistories)
            {
                wallet.Amount -= walletHistory.Amount;
                walletHistory.Redeemed = true;
                await _walletHistoryRepository.UpdateAsync(walletHistory);
            }

            await _walletRepository.UpdateAsync(wallet);

            return wallet;
        }

        public async Task<bool> RevertWalletHistoryForCancelledOrder(int customerId, string currencyCode,
            int originatedEntityId, int redeemedForEntityId)
        {
            var wallet = await GetWalletAsync(customerId, currencyCode);
            var walletHistoriesToRevert = await _walletHistoryRepository.Table
                .Where(x => x.CustomerWalletId == wallet.Id)
                .Where(x => x.OriginatedEntityId == originatedEntityId || x.RedeemedForEntityId == redeemedForEntityId)
                .ToListAsync();

            foreach (var walletHistory in walletHistoriesToRevert)
            {
                if (walletHistory.WalletHistoryType == WalletHistoryType.Reward)
                {
                    // todo how to handle and rollback reward???

                    walletHistory.ExpirationDateTime = DateTime.Now;
                    var spent = walletHistory.Amount - await GetRewardLeftBalance(wallet, walletHistory);
                    await PerformAsync(new WalletTransactionRequest
                    {
                        Amount = spent,
                        CurrencyCode = currencyCode,
                        Type = WalletHistoryType.Withdrawal,
                        Note =
                            $"revert reward for originatedEntityId:{originatedEntityId} redeemedForEntityId:{redeemedForEntityId}",
                        CustomerId = customerId,
                    });
                    await _walletHistoryRepository.UpdateAsync(walletHistory);

                    // if (walletHistory.RedeemedForEntityId == redeemedForEntityId && walletHistory.Redeemed)
                    // {
                    //     walletHistory.Redeemed = false;
                    //     walletHistory.RedeemedForEntityId = 0;
                    //     await _walletHistoryRepository.UpdateAsync(walletHistory);
                    //     //No need to change wallet amount since for every time that we use a Reward a wallet histoy with
                    //     //type rewawrd we create a wallet history with type Withdrawal for it, so we just need to 
                    //     //revert the wallet amount when removing withdral history
                    //     // wallet.Amount -= walletHistory.Amount;
                    // }
                    // else if (walletHistory.OriginatedEntityId == originatedEntityId)
                    // {
                    //     await _walletHistoryRepository.DeleteAsync(walletHistory);
                    //     wallet.Amount -= walletHistory.Amount;
                    // }
                }
                else if (walletHistory.WalletHistoryType == WalletHistoryType.Withdrawal)
                {
                    await PerformAsync(new WalletTransactionRequest
                    {
                        Amount = walletHistory.Amount,
                        CurrencyCode = currencyCode,
                        Note =
                            $"revert for originatedEntityId:{originatedEntityId} redeemedForEntityId:{redeemedForEntityId}",
                        CustomerId = customerId,
                        Type = WalletHistoryType.Deposit,
                    });
                    wallet.Amount += walletHistory.Amount;
                }
            }


            await _walletRepository.UpdateAsync(wallet);

            return true;
        }

        public async Task<IPagedList<SearchHistoryServiceResult>> SearchHistories(
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            List<int> typeIds = null,
            List<int> currencyIds = null,
            string email = "",
            string userName = "",
            string note = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var historyQuery = _walletHistoryRepository.Table;

            if (createdFromUtc.HasValue)
                historyQuery = historyQuery.Where(h => createdFromUtc.Value <= h.CreateDateTime);

            if (createdToUtc.HasValue)
                historyQuery = historyQuery.Where(h => createdToUtc.Value >= h.CreateDateTime);

            if (typeIds != null && typeIds.Any())
                historyQuery = historyQuery.Where(h => typeIds.Contains((int)h.WalletHistoryType));

            if (string.IsNullOrEmpty(note) == false)
                historyQuery = historyQuery.Where(h => !string.IsNullOrEmpty(h.Note) && h.Note.Contains(note));

            var walletQuery = _walletRepository.Table;

            if (currencyIds != null && currencyIds.Any())
                walletQuery = walletQuery.Where(w => currencyIds.Contains(w.CurrencyId));


            var query =
                from history in historyQuery
                join wallet in walletQuery on history.CustomerWalletId equals wallet.Id
                join customer in _customerRepository.Table on wallet.CustomerId equals customer.Id
                where
                    (string.IsNullOrEmpty(email) ||
                     (!string.IsNullOrEmpty(customer.Email) && customer.Email.Contains(email))) &&
                    (string.IsNullOrEmpty(userName) || (!string.IsNullOrEmpty(customer.Username) &&
                                                        customer.Username.Contains(userName)))
                join currency in _currencyRepository.Table on wallet.CurrencyId equals currency.Id
                select new SearchHistoryServiceResult
                {
                    CustomerId = customer.Id,
                    CustomerEmail = customer.Email,
                    Currency = currency,
                    History = history
                };

            query = query.OrderByDescending(result => result.History.CreateDateTime);
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }
    }
}