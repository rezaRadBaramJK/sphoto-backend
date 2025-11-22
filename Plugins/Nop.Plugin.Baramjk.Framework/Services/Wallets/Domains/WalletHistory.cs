using System;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;

namespace Nop.Plugin.Baramjk.Framework.Services.Wallets.Domains
{
    public class WalletHistory : BaseEntity
    {
        public int CustomerWalletId { get; set; }

        public decimal Amount { get; set; }

        public WalletHistoryType WalletHistoryType { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? ExpirationDateTime { get; set; }

        public int OriginatedEntityId { get; set; }

        public int RedeemedForEntityId { get; set; }

        public bool Redeemed { get; set; }
        public bool IsReverted { get; set; }

        public string Note { get; set; }
        public Guid? TxId { get; set; }

    }
}