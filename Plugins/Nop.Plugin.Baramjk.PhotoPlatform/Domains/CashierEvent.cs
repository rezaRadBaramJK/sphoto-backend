using Nop.Core;
using Nop.Core.Domain.Common;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class CashierEvent : BaseEntity, ISoftDeletedEntity
    {
        public int EventId { get; set; }
        public int CustomerId { get; set; }
        public bool Deleted { get; set; }
        public decimal OpeningFundBalanceAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public bool Active { get; set; }
        public bool IsRefundPermitted { get; set; }
    }
}