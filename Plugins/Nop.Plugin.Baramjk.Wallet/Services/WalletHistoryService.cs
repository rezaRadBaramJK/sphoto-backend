using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Domains;
using Nop.Plugin.Baramjk.Wallet.Domain;
using Nop.Plugin.Baramjk.Wallet.Models.Service;
using Nop.Plugin.Baramjk.Wallet.Plugins;
using Nop.Plugin.Baramjk.Wallet.Services.Models;
using Nop.Services.Directory;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.Wallet.Services
{
    public class WalletHistoryService
    {
       private readonly IRepository<WalletHistory> _walletHistoryRepository;
    
        private readonly IInternalWalletService _internalWalletService;

        public WalletHistoryService(IRepository<WalletHistory> walletHistoryRepository, IInternalWalletService internalWalletService)
        {
            _walletHistoryRepository = walletHistoryRepository;
            _internalWalletService = internalWalletService;
        }


        public async Task<PagedList<CustomerWalletHistoryModel>> GetCustomerWalletHistoryAsync(int customerId, string currencyCode, bool? redeemed = null, int pageNumber = 1, int pageSize = 10)
        {
            var wallet = await _internalWalletService.GetOrCreateWalletByCurrencyCodesAsync(customerId, currencyCode);
            
            var walletHistory = await _walletHistoryRepository.GetAllPagedAsync(query =>
            {
                return query.Where(x => x.CustomerWalletId == wallet.Id)
                    .Where(x => redeemed == null || x.Redeemed == redeemed)
                    .OrderByDescending(x => x.CreateDateTime);
            }, pageNumber-1, pageSize);

            var result = new List<CustomerWalletHistoryModel>();
            foreach (var history in walletHistory)
            {
                result.Add(new CustomerWalletHistoryModel
                {
                    Id = history.Id,
                    CustomerWalletId = history.CustomerWalletId,
                    Amount = history.Amount,
                    WalletHistoryType = history.WalletHistoryType,
                    //Expired = (history.ExpirationDateTime != null && history.ExpirationDateTime.Value < DateTime.UtcNow) || history.Redeemed,
                    Expired = (history.ExpirationDateTime != null && history.ExpirationDateTime.Value < DateTime.UtcNow),
                    ExpiredDateTime = history.ExpirationDateTime,
                    OriginatedEntityId = history.OriginatedEntityId,
                    CreateDateTime = history.CreateDateTime
                });
            }
            
            return new PagedList<CustomerWalletHistoryModel>(result, pageNumber, pageSize, result.Count);
        }

    }
}