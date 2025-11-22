using Nop.Plugin.Baramjk.PhotoPlatform.Domains.Types;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.CashierBalanceHistory
{
    public class CashierBalanceTransactionRequest
    {
        public int CashierEventId { get; set; }
        public decimal Amount { get; set; }
        public CashierBalanceHistoryType Type { get; set; }
        public string Note { get; set; }
    }
}