using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.CashierEvent
{
    public class CashierEventReportDetailModel
    {
        public ReservationItem Reservation { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public Product Product { get; set; }
        public Domains.CashierEvent CashierEvent { get; set; }
        public Order Order { get; set; }
        public EventDetail EventDetail { get; set; }
    }
}