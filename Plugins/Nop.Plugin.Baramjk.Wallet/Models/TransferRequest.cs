namespace Nop.Plugin.Baramjk.Wallet.Models
{
    public class TransferRequest
    {
        public int ToCustomerId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
    }
}