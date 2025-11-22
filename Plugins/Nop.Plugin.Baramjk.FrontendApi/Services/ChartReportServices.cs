using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using Nop.Plugin.Baramjk.Framework.Services.Products;
using Nop.Plugin.Baramjk.FrontendApi.Services.Models.ChartReportServices;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class ChartReportServices : IChartReportServices
    {
        private readonly IProductVisitService _productVisitService;
        private readonly IRepository<Order> _repositoryOrder;
        private readonly IRepository<OrderItem> _repositoryOrderItem;
        private readonly IRepository<Product> _repositoryProduct;
        private readonly IWorkContext _workContext;

        public ChartReportServices(IRepository<Order> repositoryOrder, IRepository<OrderItem> repositoryOrderItem,
            IRepository<Product> repositoryProduct, IWorkContext workContext, IProductVisitService productVisitService)
        {
            _repositoryOrder = repositoryOrder;
            _repositoryOrderItem = repositoryOrderItem;
            _repositoryProduct = repositoryProduct;
            _workContext = workContext;
            _productVisitService = productVisitService;
        }

        public async Task<List<ProductVisitDto>> GetProductsMostVisitedAsync(int vendorId)
        {
            var productsMostVisited = await _productVisitService.GetProductsMostVisitedAsync(vendorId, 0, 20);
            return productsMostVisited;
        }

        public IEnumerable<TopSellingProductModel> GetTopSellingProducts(int? vendorId)
        {
            var queryable = from order in _repositoryOrder.Table
                join item in _repositoryOrderItem.Table on order.Id equals item.OrderId
                join product in _repositoryProduct.Table on item.ProductId equals product.Id
                where (vendorId == null || product.VendorId == vendorId)
                      && order.CreatedOnUtc >= DateTime.Now.AddDays(-7000)
                select new
                {
                    product.Id,
                    product.Name,
                    item.Quantity,
                    item.PriceInclTax
                };

            var result = queryable
                .GroupBy(item => new { item.Id, item.Name })
                .Select(item => new
                {
                    item.Key.Id,
                    item.Key.Name,
                    Quantity = item.Sum(item2 => item2.Quantity),
                    Revenue = item.Sum(item2 => item2.PriceInclTax)
                })
                .Take(10)
                .ToList()
                .OrderByDescending(item => item.Quantity)
                .Select((item, index) => new TopSellingProductModel
                {
                    Index = index + 1,
                    Id = item.Id,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Revenue = item.Revenue
                });

            return result;
        }

        public List<ReportItem> GetProductSaleChartData(int productId, int? vendorId)
        {
            var queryable = from order in _repositoryOrder.Table
                join item in _repositoryOrderItem.Table on order.Id equals item.OrderId
                join product in _repositoryProduct.Table on item.ProductId equals product.Id
                where (vendorId == null || product.VendorId == vendorId)
                      && item.ProductId != productId
                      && order.CreatedOnUtc >= DateTime.Now.AddDays(-700)
                select new
                {
                    order.CreatedOnUtc,
                    item.PriceInclTax
                };

            var result = queryable
                .GroupBy(item => item.CreatedOnUtc.Date)
                .Select(item => new ReportItem
                {
                    Date = item.Key.ToString("yyyy/MM/dd"),
                    DateDisplay = item.Key.ToString("d dddd", Culture()),
                    Sum = item.Sum(item2 => item2.PriceInclTax)
                })
                .ToList();

            return result;
        }

        public List<ProductReportModel> GetProductsSaleChartData(int vendorId)
        {
            var queryable = from order in _repositoryOrder.Table
                join item in _repositoryOrderItem.Table on order.Id equals item.OrderId
                join product in _repositoryProduct.Table on item.ProductId equals product.Id
                where product.VendorId == vendorId && order.CreatedOnUtc >= DateTime.Now.AddDays(-700)
                select new
                {
                    item.ProductId,
                    product.Name,
                    order.CreatedOnUtc,
                    item.PriceInclTax
                };
            var list = queryable.ToList();

            var result = list
                .GroupBy(item => new { item.ProductId, item.Name })
                .Select(grouping => new ProductReportModel
                {
                    ProductId = grouping.Key.ProductId,
                    Name = grouping.Key.Name,
                    Items = grouping
                        .GroupBy(item => item.CreatedOnUtc.Date)
                        .Select(item => new ReportItem
                        {
                            Date = item.Key.ToString("yyyy/MM/dd"),
                            DateDisplay = item.Key.ToString("d dddd"),
                            Sum = item.Sum(item2 => item2.PriceInclTax)
                        })
                })
                .ToList();

            return result;
        }

        public List<ReportItem> GetOverallSales(OrderReportRequest request, int vendorId)
        {
            var format = "yyyy/MM/dd";
            switch (request.Period)
            {
                case Period.Week:
                    format = "d dddd";
                    request.End = null;
                    request.Start = DateTime.Now.AddDays(-7);
                    break;
                case Period.Month:
                    format = "M";
                    request.End = null;
                    request.Start = DateTime.Now.AddMonths(-1);
                    break;
                case Period.Year:
                    format = "yyyy/MM/dd";
                    request.End = null;
                    request.Start = DateTime.Now.AddYears(-1);
                    break;
            }

            var queryable = from order in _repositoryOrder.Table
                join item in _repositoryOrderItem.Table on order.Id equals item.OrderId
                join product in _repositoryProduct.Table on item.ProductId equals product.Id
                where product.VendorId == vendorId
                      && (request.OrderStatus == null || order.OrderStatusId == (int)request.OrderStatus)
                      && (request.PaymentStatus == null || order.PaymentStatusId == (int)request.PaymentStatus)
                      && (request.Start == null || order.CreatedOnUtc >= request.Start)
                      && (request.End == null || order.CreatedOnUtc <= request.End)
                select new
                {
                    order.CreatedOnUtc,
                    item.PriceInclTax
                };

            var result = queryable
                .GroupBy(item => item.CreatedOnUtc.Date)
                .Select(item => new ReportItem
                {
                    Date = item.Key.ToString("yyyy/MM/dd"),
                    DateDisplay = item.Key.ToString(format, Culture()),
                    Sum = item.Sum(item2 => item2.PriceInclTax)
                })
                .ToList();

            return result;
        }

        private CultureInfo Culture()
        {
            return new CultureInfo(_workContext.GetWorkingLanguageAsync().Result.LanguageCulture);
        }
    }
}