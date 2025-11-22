using System;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.Models.ChartReportServices
{
    public class ReportRequest
    {
        public Period Period { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }
}