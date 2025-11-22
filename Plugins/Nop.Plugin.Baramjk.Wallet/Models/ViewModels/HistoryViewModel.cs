using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Wallet.Models.ViewModels
{
    public record HistoryViewModel : BaseNopEntityModel
    {
        public decimal Amount { get; set; }
        
        public string CurrencyName { get; set; }
        
        public string WalletHistoryType { get; set; }
        
        public string CreateDateTime { get; set; }
        
        public string ExpirationDateTime { get; set; }
        
        public int OriginatedEntityId { get; set; }
        
        public int RedeemedForEntityId { get; set; }
        
        public bool Redeemed { get; set; }
        
        public string Note { get; set; }
        
        public int CustomerId { get; set; }

        public string CustomerEmail { get; set; }
        
        
    }
}