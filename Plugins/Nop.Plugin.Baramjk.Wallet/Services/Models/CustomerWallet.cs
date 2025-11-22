using Nop.Core.Domain.Directory;

namespace Nop.Plugin.Baramjk.Wallet.Services.Models
{
    public class CustomerWallet
    {
        public Currency Currency { get; set; }
        public Framework.Services.Wallets.Domains.Wallet Wallet { get; set; }
    }
}