using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;

namespace Nop.Plugin.Baramjk.Wallet.Services.Models
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class CustomerWalletHistoryModel
    {
        public int Id { get; set; }
        public int CustomerWalletId { get; set; }
        public decimal Amount { get; set; }
        public WalletHistoryType WalletHistoryType { get; set; }
        public bool Expired { get; set; }
        public DateTime? ExpiredDateTime { get; set; }
        public DateTime CreateDateTime { get; set; }
        public int OriginatedEntityId { get; set; }
    }
}