using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Models.Search;
using Nop.Plugin.Baramjk.Wallet.Models.ViewModels;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Plugin.Baramjk.Wallet.Services.Models;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.Wallet.Factories
{
    public class CustomerWalletFactory
    {
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWalletService _walletService;
        private readonly IInternalWalletService _internalWalletService;
        private readonly IRepository<WalletHistory> _walletHistoryRepository;
        private readonly IRepository<Framework.Services.Wallets.Domains.Wallet> _walletRepository;
        private readonly ICustomerService _customerService;
        private readonly ILogger _logger;
        public CustomerWalletFactory(
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper, IWalletService walletService, IInternalWalletService internalWalletService,
            IRepository<WalletHistory> walletHistoryRepository, IRepository<Framework.Services.Wallets.Domains.Wallet> walletRepository, ICustomerService customerService, ILogger logger)
        {
            _localizationService = localizationService;
            _currencyService = currencyService;
            _dateTimeHelper = dateTimeHelper;
            _walletService = walletService;
            _internalWalletService = internalWalletService;
            _walletHistoryRepository = walletHistoryRepository;
            _walletRepository = walletRepository;
            _customerService = customerService;
            _logger = logger;
        }

        public async Task<List<BriefWalletViewModel>> GetBriefWalletViewModel()
        {
            var data = new List<BriefWalletViewModel>();
            var wallets =await _walletHistoryRepository.Table.Select(x => x.CustomerWalletId).Distinct().ToListAsync();
            
            foreach (var walletId in wallets)
            {
                var wallet = await _walletRepository.Table.Where(x => x.Id == walletId).FirstOrDefaultAsync();
                if (wallet==default)
                {
                    await _logger.ErrorAsync($"walletid={walletId} not found");
                    continue;
                }
                var customer = await _customerService.GetCustomerByIdAsync(wallet.CustomerId);
                if (customer==default)
                {
                    await _logger.ErrorAsync($"customer={wallet.CustomerId} not found");
                    continue;
                }
                data.Add(new BriefWalletViewModel
                {
                    Balance = await _walletService.GetAvailableAmountAsync(wallet.CustomerId,wallet.CurrencyCode),
                    Currency = wallet.CurrencyCode,
                    CustomerId = wallet.CustomerId,
                    WalletId = walletId,
                    CustomerEmail = customer.Email
                });
            }

            return data;
        }
        public async Task<CustomerWalletViewModel> PrepareCustomerWalletAsync(int customerId,
            List<CustomerWallet> customerWallets)
        {
            var wallets = await customerWallets.SelectAwait(async cw =>
            {
                
                // var balanceHistories = await _walletHistoryRepository.Table
                //     .Where(x => x.CustomerWalletId == cw.Wallet.Id)
                //     .ToListAsync();
                // decimal balance = Decimal.Zero;
                // decimal activeRewards = Decimal.Zero;
                // foreach (var balanceHistory in balanceHistories)
                // {
                //     if (balanceHistory.WalletHistoryType == WalletHistoryType.Withdrawal)
                //     {
                //         balance -= balanceHistory.Amount;
                //     }
                //
                //     if (balanceHistory.WalletHistoryType == WalletHistoryType.Charge)
                //     {
                //         balance += balanceHistory.Amount;
                //     }
                //
                //     if (balanceHistory.WalletHistoryType == WalletHistoryType.Reward)
                //     {
                //         if (!balanceHistory.Redeemed)
                //         {
                //             if (balanceHistory.ExpirationDateTime == default ||
                //                 balanceHistory.ExpirationDateTime > DateTime.Now)
                //             {
                //                 balance += balanceHistory.Amount;
                //                 activeRewards += balanceHistory.Amount;
                //             } 
                //         }
                //         
                //     }
                // }
                //
                var historyTypesArray = new[] { WalletHistoryType.Reward, WalletHistoryType.Withdrawal };

                var selectListValues = await historyTypesArray.SelectAwait(async enumValue => new
                {
                    ID = Convert.ToInt32(enumValue),
                    Name = await _localizationService.GetLocalizedEnumAsync(enumValue)
                }).ToListAsync();
                var historyTypes = new SelectList(selectListValues, "ID", "Name", selectListValues.First().ID);
                
                var balance = await _walletService.GetAvailableAmountAsync(customerId,cw.Currency.CurrencyCode);
                return new WalletViewModel
                {
                    Id = cw.Wallet.Id,
                    CustomerId = customerId,
                    CurrencyName = await _localizationService.GetLocalizedAsync(cw.Currency, c => c.Name),
                    // CurrentAmount = cw.Wallet.Amount,
                    BalanceAmount = balance,
                    // ActiveRewardAmount = Decimal.Zero,
                    CurrencyId = cw.Wallet.CurrencyId,
                    HistoryTypes = historyTypes
                };
            }).ToListAsync();

            var historySearchModel = new CustomerHistorySearchModel
            {
                CustomerId = customerId,
            };

            historySearchModel.SetGridPageSize();

            return new CustomerWalletViewModel
            {
                Wallets = wallets,
                CustomerHistorySearchModel = historySearchModel
            };
        }

        public async Task<CustomerHistoryListViewModel> PrepareHistoriesListAsync(
            CustomerHistorySearchModel searchModel)
        {
            var histories = await _walletService
                .GetHistoriesByCustomerIdAsync(searchModel.CustomerId, searchModel.Page - 1, searchModel.PageSize);

            var model = new CustomerHistoryListViewModel().PrepareToGrid(searchModel, histories, () =>
            {
                return histories.Select(walletHistory => new CustomerHistoryViewModel
                {
                    Id = walletHistory.Id,
                    CurrencyCode = walletHistory.CurrencyCode,
                    Amount = walletHistory.Amount,
                    Type = walletHistory.WalletHistoryType.ToString(),
                    CreateDateTime = walletHistory.CreateDateTime.ToString("yyyy/MM/dd - hh : mm"),
                    ExpirationDateTime = walletHistory.ExpirationDateTime != null
                        ? walletHistory.ExpirationDateTime?.ToString("yyyy/MM/dd - hh : mm")
                        : string.Empty,
                    OriginatedEntityId = walletHistory.OriginatedEntityId,
                    Note = walletHistory.Note
                });
            });

            return model;
        }

        public async Task<HistoriesSearchModel> PrepareHistoriesSearchModelAsync(HistoriesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.AvailableHistoryTypes = await PrepareAvailableHistoryTypesAsync();
            searchModel.AvailableCurrencies = await PrepareCurrenciesAsync();

            searchModel.SetGridPageSize();

            return searchModel;
        }

        private async Task<IList<SelectListItem>> PrepareAvailableHistoryTypesAsync()
        {
            var availableSelectList = await WalletHistoryType.Spend.ToSelectListAsync(false);
            var items = availableSelectList.ToList();
            await InsertDefaultItemAsync(items);
            return items;
        }

        private async Task<IList<SelectListItem>> PrepareCurrenciesAsync()
        {
            var allCurrencies = await _currencyService.GetAllCurrenciesAsync();
            var items = allCurrencies.Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
                .ToList();
            await InsertDefaultItemAsync(items);
            return items;
        }

        private async Task InsertDefaultItemAsync(IList<SelectListItem> items, bool isSelected = true)
        {
            var defaultItemText = await _localizationService.GetResourceAsync("Admin.Common.All");
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = "0", Selected = isSelected });
        }

        public async Task<HistoriesListViewModel> PrepareHistoriesListViewModelAsync(HistoriesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue
                ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value,
                    await _dateTimeHelper.GetCurrentTimeZoneAsync());

            var endDateValue = !searchModel.EndDate.HasValue
                ? null
                : (DateTime?)_dateTimeHelper
                    .ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync())
                    .AddDays(1);

            var historyTypeIds = (searchModel.HistoryTypeIds?.Contains(0) ?? true)
                ? null
                : searchModel.HistoryTypeIds.ToList();
            var currencyIds = (searchModel.CurrencyIds?.Contains(0) ?? true) ? null : searchModel.CurrencyIds.ToList();

            var searchHistoriesResult = await _internalWalletService.SearchHistories(
                startDateValue,
                endDateValue,
                historyTypeIds,
                currencyIds,
                searchModel.SearchEmail,
                searchModel.SearchUsername,
                searchModel.SearchNote,
                searchModel.Page - 1,
                searchModel.PageSize);

            return await new HistoriesListViewModel().PrepareToGridAsync(searchModel, searchHistoriesResult, () =>
            {
                return searchHistoriesResult.SelectAwait(async r =>
                {
                    return new HistoryViewModel
                    {
                        Id = r.History.Id,
                        Amount = r.History.Amount,
                        CurrencyName = await _localizationService.GetLocalizedAsync(r.Currency, c => c.Name),
                        WalletHistoryType =
                            await _localizationService.GetLocalizedEnumAsync(r.History.WalletHistoryType),
                        CreateDateTime = r.History.CreateDateTime.ToString("yyyy/MM/dd hh:mm"),
                        ExpirationDateTime = r.History.ExpirationDateTime?.ToString("yyyy/MM/dd hh:mm"),
                        OriginatedEntityId = r.History.OriginatedEntityId,
                        RedeemedForEntityId = r.History.RedeemedForEntityId,
                        Redeemed = r.History.Redeemed,
                        Note = r.History.Note,
                        CustomerId = r.CustomerId,
                        CustomerEmail = r.CustomerEmail
                    };
                });
            });
        }
    }
}