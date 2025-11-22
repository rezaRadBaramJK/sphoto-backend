using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Baramjk.Framework.Services.Wallets.Domains
{
    public class Wallet : BaseEntity, ILocalizedEntity
    {
        public int CustomerId { get; set; }
        public string CurrencyCode { get; set; }
        public int CurrencyId { get; set; }
        public decimal Amount { get; set; }
        public decimal LockAmount { get; set; }
        public bool IsLocked { get; set; }
    }
}