using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reports
{
    public class SupervisorReportApiParams : ReportBaseApiParam
    {
        public bool ShowActors { get; set; }
        public List<int> SelectedActorIds { get; set; }
        public bool UsedTicketsReportOnly { get; set; }
    }
}