namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Reports
{
    public class TechnicianDailyReportModel
    {
        public string DayOfWeek { get; set; }

        public TechnicianDailyTechnicianReportModel[] TechnicianReports { get; set; }
    }
}