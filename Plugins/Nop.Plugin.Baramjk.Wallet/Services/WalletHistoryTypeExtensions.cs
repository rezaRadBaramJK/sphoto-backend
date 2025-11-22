using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;

namespace Nop.Plugin.Baramjk.Wallet.Services
{
    public static class WalletHistoryTypeExtensions
    {
        public static WalletHistoryType Negate(this WalletHistoryType state)
        {
            if (GetDepositTypes().Contains(state))
            {
                return WalletHistoryType.Withdrawal;
            }

            if (GetWithdrawalTypes().Contains(state))
            {
                return WalletHistoryType.Deposit;

            }

            return WalletHistoryType.None;
        } 
        public static List<WalletHistoryType> GetDepositTypes()
        {
            return new List<WalletHistoryType>()
            {
                WalletHistoryType.Charge,
                WalletHistoryType.Deposit,
                WalletHistoryType.TransferReceive,
                WalletHistoryType.Reward,
                WalletHistoryType.UnLocking,
                WalletHistoryType.Extra,
                WalletHistoryType.FreeAd,
            };
        }

        public static List<WalletHistoryType> GetWithdrawalTypes()
        {
            return new List<WalletHistoryType>()
            {
                WalletHistoryType.Withdrawal,
                WalletHistoryType.TransferSend,
                WalletHistoryType.Locking,
            };
        }
    }
}