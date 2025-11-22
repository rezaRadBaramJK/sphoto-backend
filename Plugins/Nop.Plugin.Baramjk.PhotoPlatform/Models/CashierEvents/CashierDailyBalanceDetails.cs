using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.CashierEvents
{
    public class CashierDailyBalanceDetails
    {
        public Customer Customer { get; set; }
        
        public CashierDailyBalance CashierDailyBalance { get; set; }
    }
}