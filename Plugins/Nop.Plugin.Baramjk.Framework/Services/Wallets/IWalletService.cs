using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace Nop.Plugin.Baramjk.Framework.Services.Wallets
{
    public interface IWalletService
    {
        Task<decimal> GetAvailableAmountAsync(int customerId, string currencyCode);
        Task<bool> CanPerformAsync(WalletTransactionRequest request);
        Task<bool> PerformAsync(WalletTransactionRequest request);
        Task<bool> CanRevertAsync(WalletHistory historyToRevert,string comment);
        Task<bool> CanRevertTransferAsync(Guid txid,string comment);

        Task<bool> RevertAsync(WalletHistory historyToRevert,string comment);
        Task<bool> RevertTransferAsync(Guid txid,string comment);

        Task<List<WalletModel>> GetCustomerWalletsModelAsync(int customerId);
        Task<bool> CanTransferAsync(WalletTransferTransactionRequest request);
        Task<bool> TransferAsync(WalletTransferTransactionRequest request);
        string DefaultCurrencyCode { get; }
        bool IsForceUseWalletCredit();
        List<WalletHistoryType> GetDepositTypes();
        List<WalletHistoryType> GetWithdrawalTypes();

        Task<List<WalletHistory>> Search(
            List<WalletHistoryType> types = default, int pageNumber = 1, int pageSize = 10,
            DateTime fromDateTime = default, DateTime toDateTime = default, List<int> customerWalletIds = default);

        Task<IPagedList<HistoryWithCurrencyCodeServiceResult>> GetHistoriesByCustomerIdAsync(int customerId,
            int pageIndex = 0, int pageSize = 10);

        Task<decimal> CalculateRewardWalletHistories(Wallet wallet, WalletHistoryType type, decimal amount,
            int orderId);
    }
}