using Nop.Core.Domain.Directory;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Domains;
using Nop.Plugin.Baramjk.Wallet.Domain;

namespace Nop.Plugin.Baramjk.Wallet.Models.Service
{
    public class SearchHistoryServiceResult
    {
        public int CustomerId { get; set; }
        
        public string CustomerEmail { get; set; }
        
        public Currency Currency { get; set; }
        
        public WalletHistory History { get; set; }
        
    }
}