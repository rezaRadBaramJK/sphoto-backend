using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Reports
{
    public class SupervisorRevenueReportOverviewDto : RevenueReportOverviewBaseDto
    {
        public int TotalActors { get; set; }
        public bool ShowTickets { get; set; }
        public string ProductionShare { get; set; }
        public string SPhotoGroupShare { get; set; }

        public List<SupervisorActorRevenueReportDto> ActorsReports { get; set; }
    }
}