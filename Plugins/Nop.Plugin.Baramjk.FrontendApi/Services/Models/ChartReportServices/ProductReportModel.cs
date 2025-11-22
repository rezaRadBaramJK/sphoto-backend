using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.Models.ChartReportServices
{
    public class ProductReportModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public IEnumerable<ReportItem> Items { get; set; }
    }
}