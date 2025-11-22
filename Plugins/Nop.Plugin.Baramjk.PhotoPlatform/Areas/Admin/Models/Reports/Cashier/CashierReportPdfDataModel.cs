using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Cashier
{
    public class CashierReportPdfDataModel
    {
        public string EventDate { get; set; }
        public string EventTime { get; set; }
        public string EventName { get; set; }
        
        public bool ShowTicketsCount { get; set; }
        public List<CashierReportPdfReservationDataModel> Reservations { get; set; }
    }
}