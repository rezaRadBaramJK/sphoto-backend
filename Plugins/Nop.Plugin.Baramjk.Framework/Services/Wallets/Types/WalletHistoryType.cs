using System;
using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Framework.Services.Wallets.Types
{
    public enum WalletHistoryType
    {
        Spend = 1,
        Locking = 2,
        UnLocking = 3,
        Charge = 4,
        Extra = 5,
        Deposit = 6,
        Withdrawal = 7,
        TransferSend = 8,
        TransferReceive = 9,
        FreeAd = 10,
        Reward = 11,
        None=99
    }
}