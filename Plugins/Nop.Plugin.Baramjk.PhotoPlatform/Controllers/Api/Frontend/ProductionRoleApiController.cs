using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reports;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;


namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Frontend
{
    public class ProductionRoleApiController : BaseBaramjkApiController
    {
        private readonly ReportsFactory _reportsFactory;
        private readonly ReportsService _reportsService;
        private readonly IWorkContext _workContext;
        private readonly ExportService _exportService;
        private readonly ProductionEventService _productionEventService;


        public ProductionRoleApiController(
            ReportsFactory reportsFactory,
            ReportsService reportsService,
            IWorkContext workContext,
            ExportService exportService,
            ProductionEventService productionEventService)
        {
            _reportsFactory = reportsFactory;
            _reportsService = reportsService;
            _workContext = workContext;
            _exportService = exportService;
            _productionEventService = productionEventService;
        }

        private string GetPdfViewPath(string viewName)
        {
            var systemName = GetType().Assembly.GetName().Name?.Replace("Nop.Plugin.", "");
            return $"~/Plugins/{systemName}/Views/Reports/PDF/{viewName}.cshtml";
        }

        [AuthorizeApi(PermissionProvider.ProductionName)]
        [HttpGet("/FrontendApi/PhotoPlatform/Production/ReportFilters/")]
        public async Task<IActionResult> GetProductionRoleReportFiltersAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var data = await _productionEventService.GetProductionEventsFullDetailsAsync(customer.Id);
            var result = await _reportsFactory.PrepareReportFiltersAsync(data);
            return ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.ProductionName)]
        [HttpPost("/FrontendApi/PhotoPlatform/Production/Report/")]
        public async Task<IActionResult> GetFilteredReportAsync(ProductionRoleReportApiParams apiParams)
        {
            if (apiParams.EventId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid eventId");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            var rawData = await _reportsService.GetProductionTicketRevenueDataAsync(
                apiParams.EventId,
                customer.Id,
                apiParams.ShowActors ? apiParams.SelectedActorIds : new List<int>(),
                apiParams.Date,
                apiParams.TimeSlotId
            );

            if (rawData == null || rawData.Any() == false)
            {
                return ApiResponseFactory.Success();
            }

            var result = await _reportsFactory.PrepareProductionRoleReportAsync(rawData);

            return ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.ProductionName)]
        [HttpPost("/FrontendApi/PhotoPlatform/Production/Report/PDF/Generate")]
        public async Task<IActionResult> GeneratePdfReportAsync(ProductionRoleReportApiParams apiParams)
        {
            if (apiParams.EventId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid eventId");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            var rawData = await _reportsService.GetProductionTicketRevenueDataAsync(
                apiParams.EventId,
                customer.Id,
                apiParams.ShowActors ? apiParams.SelectedActorIds : new List<int>(),
                apiParams.Date,
                apiParams.TimeSlotId
            );

            if (rawData == null || rawData.Any() == false)
            {
                return ApiResponseFactory.Success();
            }

            var preparedItems = await _reportsFactory.PrepareProductionReportDataAsync(rawData);

            return new ViewAsPdf(GetPdfViewPath("ProductionReport"), preparedItems)
            {
                ContentDisposition = ContentDisposition.Attachment,
                FileName = $"ProductionReport-{DateTime.Now:s}.pdf",
                PageSize = Size.A4,
                PageMargins = new Margins(2, 2, 2, 2)
            };
        }

        [AuthorizeApi(PermissionProvider.ProductionName)]
        [HttpPost("/FrontendApi/PhotoPlatform/Production/Report/Excel/Generate")]
        public async Task<IActionResult> GenerateExcelReportAsync(ProductionRoleReportApiParams apiParams)
        {
            if (apiParams.EventId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid eventId");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            var rawData = await _reportsService.GetProductionTicketRevenueDataAsync(
                apiParams.EventId,
                customer.Id,
                apiParams.ShowActors ? apiParams.SelectedActorIds : new List<int>(),
                apiParams.Date,
                apiParams.TimeSlotId
            );

            if (rawData == null || rawData.Any() == false)
            {
                return ApiResponseFactory.Success();
            }

            var preparedItems = await _reportsFactory.PrepareProductionReportDataAsync(rawData);


            var bytes = await _exportService.ExportProductionReportAsync(preparedItems, apiParams.ShowActors);

            return File(bytes, MimeTypes.TextXlsx, $"production-report-{DateTime.Now:s}.xlsx");
        }


        [AuthorizeApi(PermissionProvider.ProductionName, PermissionProvider.SupervisorName)]
        [HttpPost("/FrontendApi/PhotoPlatform/ActorsReport/PDF/Generate")]
        public async Task<IActionResult> GenerateActorsPdfReportAsync(ProductionRoleReportApiParams apiParams)
        {
            if (apiParams.EventId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid eventId");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            var rawData = await _reportsService.GetProductionTicketRevenueDataAsync(
                apiParams.EventId,
                customer.Id,
                apiParams.ShowActors ? apiParams.SelectedActorIds : new List<int>(),
                apiParams.Date,
                apiParams.TimeSlotId
            );

            if (rawData == null || rawData.Any() == false)
            {
                return ApiResponseFactory.Success();
            }

            var preparedItems = await _reportsFactory.PrepareActorReportModelAsync(rawData);

            return new ViewAsPdf(GetPdfViewPath("ActorReport"), preparedItems)
            {
                ContentDisposition = ContentDisposition.Attachment,
                FileName = $"ActorReport-{DateTime.Now:s}.pdf",
                PageSize = Size.A4,
                PageMargins = new Margins(2, 2, 2, 2)
            };
        }
    }
}