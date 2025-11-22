using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using Nop.Plugin.Baramjk.FrontendApi.Services.Models.ChartReportServices;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public interface IChartReportServices
    {
        Task<List<ProductVisitDto>> GetProductsMostVisitedAsync(int vendorId);
        IEnumerable<TopSellingProductModel> GetTopSellingProducts(int? vendorId);
        List<ReportItem> GetProductSaleChartData(int productId, int? vendorId);
        List<ReportItem> GetOverallSales(OrderReportRequest request, int vendorId);
        List<ProductReportModel> GetProductsSaleChartData(int vendorId);
    }
}