using System;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;

namespace Nop.Plugin.Baramjk.Framework.Services.Wallets
{
    public class HistoryWithCurrencyCodeServiceResult
    {
        public int Id { get; set; }
        public string CurrencyCode { get; set; }
        public int CustomerWalletId { get; set; }
        public decimal Amount { get; set; }
        public WalletHistoryType WalletHistoryType { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? ExpirationDateTime { get; set; }
        public int OriginatedEntityId { get; set; }
        public int RedeemedForEntityId { get; set; }
        public bool Redeemed { get; set; }
        public string Note { get; set; }
    }
}