using System;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;

namespace Nop.Plugin.Baramjk.Framework.Services.Wallets.Models
{
    public class WalletTransactionRequest
    {
        public int CustomerId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public WalletHistoryType Type { get; set; }
        public object? ExtraData { get; set; }
        public int OriginatedEntityId { get; set; }
        public int RedeemedForEntityId { get; set; }
        public DateTime? ExpireDateTime { get; set; }
        public Guid? TxId { get; set; }
        public bool IsRevert { get; set; }
        public string Note { get; set; }


    }
}