namespace Nop.Plugin.Baramjk.Framework.Services.Wallets.Models
{
    public class WalletTransferTransactionRequest
    {
        public int FromCustomerId { get; set; }
        public int ToCustomerId { get; set; }
        public string CurrencyCode { get; set; }
        public string Note { get; set; }
        public decimal Amount { get; set; }
        public object? ExtraData { get; set; }
        public int OriginatedEntityId { get; set; }
        public int RedeemedForEntityId { get; set; }
    }
}