using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains.Types;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class CashierBalanceHistory: BaseEntity, ISoftDeletedEntity
    {
        public decimal Amount { get; set; }
        public int CashierEventId { get; set; }
        public string Note { get; set; }
        public CashierBalanceHistoryType Type { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public bool Deleted { get; set; }
    }
}