using System.Collections.Generic;
using Nop.Plugin.Baramjk.Wallet.Models.Search;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Wallet.Models.ViewModels
{
    public record CustomerWalletViewModel : BaseNopEntityModel
    {
        public List<WalletViewModel> Wallets { get; set; }
        
        public CustomerHistorySearchModel CustomerHistorySearchModel { get; set; }
        
    }
}