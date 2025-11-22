using Nop.Core;

namespace Nop.Plugin.Baramjk.Wallet.Domain
{
    public class WalletItemPackage : BaseEntity
    {
        public int WalletPackageId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
    }
}