using Nop.Core;

namespace Nop.Plugin.Baramjk.Wallet.Domain
{
    public class WalletTranslation : BaseEntity
    {
        public int CustomerId { get; set; }
        public WalletTranslationStatus Status { get; set; }
        public decimal AmountToPay { get; set; }
        public string PaymentId { get; set; }
        public string InvoiceId { get; set; }
        public string Data { get; set; }
    }
}