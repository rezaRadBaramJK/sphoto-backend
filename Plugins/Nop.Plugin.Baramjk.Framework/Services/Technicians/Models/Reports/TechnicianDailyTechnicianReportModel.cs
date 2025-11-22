using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Reports
{
    public class TechnicianDailyTechnicianReportModel
    {
        public string TechnicianName { get; set; }
        
        public IList<TechnicianDailyReportItemModel> Items { get; set; }

    }
}