using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Wallet.Services.Models
{
    public class PackageModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public List<WalletItemPackageModel> Items { get; set; }
    }

    public class WalletItemPackageModel
    {
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
    }
}