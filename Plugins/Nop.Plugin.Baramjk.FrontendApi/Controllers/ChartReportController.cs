using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using Nop.Plugin.Baramjk.FrontendApi.Services;
using Nop.Plugin.Baramjk.FrontendApi.Services.Models.ChartReportServices;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class VendorChartReportController : BaseNopWebApiFrontendController
    {
        private readonly IChartReportServices _chartReportServices;
        private readonly IWorkContext _workContext;

        public VendorChartReportController(IChartReportServices chartReportServices, IWorkContext workContext)
        {
            _chartReportServices = chartReportServices;
            _workContext = workContext;
        }

        private int VendorId => _workContext.GetCurrentVendorAsync().Result.Id;

        [HttpGet("GetProductsMostVisited")]
        [ProducesResponseType(typeof(List<ProductVisitDto>), StatusCodes.Status200OK)]
        public async Task<List<ProductVisitDto>> GetProductsMostVisitedAsync()
        {
            return await _chartReportServices.GetProductsMostVisitedAsync(VendorId);
        }

        [HttpGet("GetTopSellingProducts")]
        [ProducesResponseType(typeof(IEnumerable<TopSellingProductModel>), StatusCodes.Status200OK)]
        public IEnumerable<TopSellingProductModel> GetTopSellingProducts()
        {
            return _chartReportServices.GetTopSellingProducts(VendorId);
        }

        [HttpGet("GetProductSaleChartData")]
        [ProducesResponseType(typeof(List<ReportItem>), StatusCodes.Status200OK)]
        public List<ReportItem> GetProductSaleChartData(int productId)
        {
            return _chartReportServices.GetProductSaleChartData(productId, VendorId);
        }

        [HttpGet("GetProductsSaleChartData")]
        [ProducesResponseType(typeof(List<ReportItem>), StatusCodes.Status200OK)]
        public List<ProductReportModel> GetProductSaleChartData()
        {
            return _chartReportServices.GetProductsSaleChartData(VendorId);
        }

        [HttpGet("GetOverallSales")]
        [ProducesResponseType(typeof(List<ReportItem>), StatusCodes.Status200OK)]
        public List<ReportItem> GetOverallSales([FromQuery] OrderReportRequest request)
        {
            return _chartReportServices.GetOverallSales(request, VendorId);
        }
    }
}