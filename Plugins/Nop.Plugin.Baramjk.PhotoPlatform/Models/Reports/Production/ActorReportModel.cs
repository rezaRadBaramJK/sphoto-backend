using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.Production
{
    public class ActorReportModel
    {
        public List<DailyEventData> GroupedByDate { get; set; } = new();
        public bool ShowTickets { get; set; }
    }
}