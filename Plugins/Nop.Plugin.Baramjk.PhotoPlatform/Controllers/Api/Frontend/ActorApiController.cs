using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Actors;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reports;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Frontend
{
    public class ActorApiController : BaseBaramjkApiController
    {
        private readonly ReservationItemService _reservationItemService;
        private readonly IWorkContext _workContext;
        private readonly ActorService _actorService;
        private readonly ReportsFactory _reportsFactory;
        private readonly ExportService _exportService;


        public ActorApiController(ReservationItemService reservationItemService,
            IWorkContext workContext,
            ActorService actorService,
            ReportsFactory reportsFactory,
            ExportService exportService)
        {
            _reservationItemService = reservationItemService;
            _workContext = workContext;
            _actorService = actorService;
            _reportsFactory = reportsFactory;
            _exportService = exportService;
        }

        private string GetPdfViewPath(string viewName)
        {
            var systemName = GetType().Assembly.GetName().Name?.Replace("Nop.Plugin.", "");
            return $"~/Plugins/{systemName}/Views/Reports/PDF/{viewName}.cshtml";
        }

        [AuthorizeApi(PermissionProvider.ActorName)]
        [HttpGet("/FrontendApi/PhotoPlatform/Actor/Reservations/")]
        public async Task<IActionResult> GetActorEvents()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var result = await _reservationItemService.GetActorReservationsAsync(customer);
            return ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.ActorName)]
        [HttpPut("/FrontendApi/PhotoPlatform/Actor/UpdateInfo/")]
        public async Task<IActionResult> UpdateActorInfoAsync([FromBody] UpdateActorInfoApiParams apiParams)
        {
            if (apiParams == null)
            {
                return ApiResponseFactory.BadRequest();
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            var actor = await _actorService.GetByCustomerIdAsync(customer.Id);

            if (actor == null)
            {
                return ApiResponseFactory.BadRequest("No such actor");
            }

            await _actorService.UpdateActorInfoAsync(apiParams, customer, actor);
            return ApiResponseFactory.Success();
        }


        [AuthorizeApi(PermissionProvider.ActorName)]
        [HttpGet("/FrontendApi/PhotoPlatform/Actor/Details/")]
        public async Task<IActionResult> GetActorDetailsAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var result = await _actorService.GetActorDetailsAsync(customer.Id);
            return ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.ActorName)]
        [HttpGet("/FrontendApi/PhotoPlatform/Actor/ReportFilters/")]
        public async Task<IActionResult> GetActorEventsAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var data = await _actorService.GetActorDetailedEventsAsync(customerId: customer.Id);
            var result = await _reportsFactory.PrepareActorReportFiltersDtoAsync(data);
            return ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.ActorName)]
        [HttpPost("/FrontendApi/PhotoPlatform/Actor/Report/")]
        public async Task<IActionResult> GetFilteredReportAsync(ActorReportApiParams apiParams)
        {
            if (apiParams.EventId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid eventId");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            var rawData = await _actorService.GetActorReportData(customer.Id, apiParams.EventId, apiParams.Date, timeSlotId: apiParams.TimeSlotId);
            var result = await _reportsFactory.PrepareActorRevenueReportAsync(rawData);

            return ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.ActorName)]
        [HttpPost("/FrontendApi/PhotoPlatform/Actor/Report/PDF/Generate")]
        public async Task<IActionResult> GeneratePdfReportAsync(ActorReportApiParams apiParams)
        {
            if (apiParams.EventId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid eventId");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            var rawData = await _actorService.GetActorRevenueDataAsync(
                customer.Id,
                date: apiParams.Date,
                timeSlotId: apiParams.TimeSlotId,
                eventId: apiParams.EventId);

            if (rawData == null || rawData.Any() == false)
            {
                return ApiResponseFactory.Success();
            }

            var result = await _reportsFactory.PrepareActorTimeSlotRevenueReportAsync(rawData);

            if (result == null || result.GroupedByDate == null || result.GroupedByDate.Any() == false)
            {
                return ApiResponseFactory.Success();
            }


            return new ViewAsPdf(GetPdfViewPath("ActorTimeSlotRevenueReport"), result)
            {
                ContentDisposition = ContentDisposition.Attachment,
                FileName = $"ActorTimeSlotRevenueReport-{DateTime.Now:s}.pdf",
                PageSize = Size.A5,
                PageMargins = new Margins(2, 2, 2, 2)
            };
        }

        [AuthorizeApi(PermissionProvider.ActorName)]
        [HttpPost("/FrontendApi/PhotoPlatform/Actor/Report/Excel/Generate")]
        public async Task<IActionResult> GenerateExcelReportAsync(ActorReportApiParams apiParams)
        {
            if (apiParams.EventId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid eventId");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            var rawData = await _actorService.GetActorRevenueDataAsync(
                customer.Id,
                date: apiParams.Date,
                timeSlotId: apiParams.TimeSlotId,
                eventId: apiParams.EventId);

            if (rawData == null || rawData.Any() == false)
            {
                return ApiResponseFactory.Success();
            }

            var result = await _reportsFactory.PrepareActorTimeSlotRevenueReportAsync(rawData);

            if (result == null || result.GroupedByDate == null || result.GroupedByDate.Any() == false)
            {
                return ApiResponseFactory.Success();            }


            var bytes = await _exportService.ExportActorRevenueAsync(result);

            return File(bytes, MimeTypes.TextXlsx, $"tickets-report-{DateTime.Now:s}.xlsx");
        }
    }
}