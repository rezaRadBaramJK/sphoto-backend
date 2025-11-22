using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Sales
{
    public class SalesEachDayDataPdfDataModel
    {
        public int DayNumber { get; set; }
        public List<SalesReservationDetailsDataModel> ReservationsDetails { get; set; }
        
        public int TotalNumberOfTickets { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalProductionShare { get; set; }
        public decimal TotalActorShare { get; set; }
        public decimal TotalPhotoShootShare { get; set; }
    }
}