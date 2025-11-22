using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Baramjk.Wallet.Domain
{
    public class WalletPackage : BaseEntity, ILocalizedEntity
    {
        public string Name { get; set; }
    }
}