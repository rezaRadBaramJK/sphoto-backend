namespace Nop.Plugin.Baramjk.Wallet.Services.Models
{
    public class WalletPackageItemAddModel
    {
        public WalletPackageItemAddModel(string currencyCode, decimal amount)
        {
            CurrencyCode = currencyCode;
            Amount = amount;
        }

        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
    }
}