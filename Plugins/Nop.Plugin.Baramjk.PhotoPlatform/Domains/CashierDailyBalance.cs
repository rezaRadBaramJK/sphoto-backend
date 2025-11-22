using System;
using Nop.Core;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class CashierDailyBalance : BaseEntity
    {
        public int CashierEventId { get; set; }
        public DateTime Day { get; set; }
        public decimal OpeningFundBalanceAmount { get; set; }
    }
}