namespace Nop.Plugin.Baramjk.Wallet.Models.ViewModels
{
    public class BriefWalletViewModel
    {
        public int CustomerId { get; set; }
        public int WalletId { get; set; }
        public string CustomerEmail { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; }
    }
}