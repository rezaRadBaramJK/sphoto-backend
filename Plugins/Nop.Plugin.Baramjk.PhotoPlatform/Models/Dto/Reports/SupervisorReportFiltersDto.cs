using System.Collections.Generic;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Actors;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Reports
{
    public class SupervisorReportFiltersDto : ReportFiltersBaseDto
    {
        public List<ActorInfoDto> Actors { get; set; }
    }
}