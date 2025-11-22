using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Reports
{
    public class ProductionRoleReportDto : RevenueReportOverviewBaseDto
    {
        public int TotalActors { get; set; }
        public bool ShowTickets { get; set; }
        public string ProductionShare { get; set; }

        public List<ProductionActorRevenueReportDto> ActorsReports { get; set; }
    }
}