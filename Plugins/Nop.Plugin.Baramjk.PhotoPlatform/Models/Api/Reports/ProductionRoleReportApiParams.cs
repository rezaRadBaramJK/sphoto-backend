using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reports
{
    public class ProductionRoleReportApiParams : ReportBaseApiParam
    {
        public bool ShowActors { get; set; }
        public List<int> SelectedActorIds { get; set; }
    }
}