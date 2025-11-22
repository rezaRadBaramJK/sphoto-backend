using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.Supervisor
{
    public class SupervisorReportModel
    {
        public List<DateGroupedData> GroupedByDate { get; set; } = new();
        public int TotalPhotoCount { get; set; }
        public decimal TotalPhotoPrice { get; set; }
        
        public int TotalCameraManPhotoCount { get; set; }
        
        public int TotalCustomerMobilePhotoCount { get; set; }
        public decimal GeneralActorShare { get; set; }
        public decimal GeneralProductionShare { get; set; }
        public decimal GeneralPhotoShootShare { get; set; }
        public decimal GeneralPhotoPrice { get; set; }
        public string ReportType { get; set; }
        public bool ShowActors { get; set; }
    }
}