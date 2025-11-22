using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Sales
{
    public class SalesReportPdfDataModel
    {
        public string EventName { get; set; }
        public List<SalesEachDayDataPdfDataModel> EachDayData { get; set; }
        
        public int TotalNumberOfTickets { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        public decimal TotalProductionShare { get; set; }
        
        public decimal TotalActorShare { get; set; }
        
        public decimal TotalPhotoShootShare { get; set; } 
       
    }

}