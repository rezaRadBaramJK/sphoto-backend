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
    public class SupervisorApiController : BaseBaramjkApiController
    {
        private readonly EventFactory _eventFactory;
        private readonly TimeSlotService _timeSlotService;
        private readonly ReportsService _reportsService;
        private readonly ReportsFactory _reportsFactory;
        private readonly IWorkContext _workContext;
        private readonly SupervisorEventService _supervisorEventService;
        private readonly ExportService _exportService;

        public SupervisorApiController(
            EventFactory eventFactory,
            TimeSlotService timeSlotService,
            ReportsService reportsService,
            ReportsFactory reportsFactory,
            IWorkContext workContext,
            SupervisorEventService supervisorEventService, 
            ExportService exportService)
        {
            _eventFactory = eventFactory;
            _timeSlotService = timeSlotService;
            _reportsService = reportsService;
            _reportsFactory = reportsFactory;
            _workContext = workContext;
            _supervisorEventService = supervisorEventService;
            _exportService = exportService;
        }

        private string GetPdfViewPath(string viewName)
        {
            var systemName = GetType().Assembly.GetName().Name?.Replace("Nop.Plugin.", "");
            return $"~/Plugins/{systemName}/Views/Reports/PDF/{viewName}.cshtml";
        }

        [AuthorizeApi(PermissionProvider.SupervisorName)]
        [HttpGet("/FrontendApi/PhotoPlatform/Supervisor/Event/")]
        public async Task<IActionResult> GetEventsAsync()
        {
            var supervisor = await _workContext.GetCurrentCustomerAsync();
            var events = await _supervisorEventService.GetSupervisorEventsAsync(supervisor.Id);
            var result = await _eventFactory.PrepareEventsAsync(events);
            return ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.SupervisorName)]
        [HttpGet("/FrontendApi/PhotoPlatform/Supervisor/Event/{id:int}")]
        public async Task<IActionResult> GetEventByIdAsync(int id)
        {
            if (id < 1)
            {
                return ApiResponseFactory.BadRequest("Provided id is invalid");
            }

            var supervisor = await _workContext.GetCurrentCustomerAsync();

            var eventDetails = await _supervisorEventService.GetSupervisorEventFullDetailsAsync(id, supervisor.Id, includeDeactivatedTimeSlots: true);

            if (eventDetails == null)
            {
                return ApiResponseFactory.BadRequest("Event not found");
            }

            var result = await _eventFactory.PrepareSupervisorEventDtoAsync(eventDetails, false);

            return result == null ? ApiResponseFactory.BadRequest("Event not found") : ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.SupervisorName)]
        [HttpPut("/FrontendApi/PhotoPlatform/Supervisor/TimeSlot/Deactivate/{id:int}")]
        public async Task<IActionResult> DeactivateTimeSlotAsync(int id)
        {
            if (id < 1)
            {
                return ApiResponseFactory.BadRequest("Provided id is invalid");
            }


            var timeSlot = await _timeSlotService.GetByIdAsync(id);

            if (timeSlot == null)
            {
                return ApiResponseFactory.BadRequest("Time slot not found");
            }

            var supervisorEvent = await _supervisorEventService.GetByEventIdAsync(timeSlot.EventId);

            var supervisor = await _workContext.GetCurrentCustomerAsync();

            if (supervisorEvent == null || supervisorEvent.CustomerId != supervisor.Id)
            {
                return ApiResponseFactory.BadRequest("Supervisor has not access to this event");
            }

            timeSlot.Active = false;
            await _timeSlotService.UpdateAsync(timeSlot);

            return ApiResponseFactory.Success();
        }


        [AuthorizeApi(PermissionProvider.SupervisorName)]
        [HttpGet("/FrontendApi/PhotoPlatform/Supervisor/ReportFilters/")]
        public async Task<IActionResult> GetSupervisorReportFiltersAsync()
        {
            var supervisor = await _workContext.GetCurrentCustomerAsync();
            var data = await _supervisorEventService.GetEventsFullDetailsAsync(supervisor.Id);
            var result = await _reportsFactory.PrepareReportFiltersAsync(data);
            return ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.SupervisorName)]
        [HttpPost("/FrontendApi/PhotoPlatform/Supervisor/Report/")]
        public async Task<IActionResult> GenerateReportAsync(SupervisorReportApiParams apiParams)
        {
            if (apiParams.EventId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid eventId");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            var rawData = await _reportsService.GetSupervisorTicketRevenueDataAsync(
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

            var result = await _reportsFactory.PrepareSupervisorReportAsync(apiParams.UsedTicketsReportOnly, rawData);

            return ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.SupervisorName)]
        [HttpPost("/FrontendApi/PhotoPlatform/Supervisor/Report/PDF/Generate")]
        public async Task<IActionResult> GeneratePdfReportAsync(SupervisorReportApiParams apiParams)
        {
            if (apiParams.EventId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid eventId");
            }


            var customer = await _workContext.GetCurrentCustomerAsync();
            var rawData = await _reportsService.GetSupervisorTicketRevenueDataAsync(
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


            var preparedItems =
                await _reportsFactory.PrepareSupervisorReportDataAsync(rawData, apiParams.UsedTicketsReportOnly, apiParams.ShowActors);


            return new ViewAsPdf(GetPdfViewPath("SupervisorReport"), preparedItems)
            {
                ContentDisposition = ContentDisposition.Attachment,
                FileName = $"SupervisorReport-{DateTime.Now:s}.pdf",
                PageSize = Size.A4,
                PageMargins = new Margins(2, 2, 2, 2)
            };
        }
        [AuthorizeApi(PermissionProvider.SupervisorName)]
        [HttpPost("/FrontendApi/PhotoPlatform/Supervisor/Report/Excel/Generate")]
        public async Task<IActionResult> GenerateExcelReportAsync(SupervisorReportApiParams apiParams)
        {
            if (apiParams.EventId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid eventId");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            var rawData = await _reportsService.GetSupervisorTicketRevenueDataAsync(
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

            var preparedItems =
                await _reportsFactory.PrepareSupervisorReportDataAsync(rawData, apiParams.UsedTicketsReportOnly, apiParams.ShowActors);


            var bytes = await _exportService.ExportSupervisorReportAsync(preparedItems, apiParams.ShowActors);

            return File(bytes, MimeTypes.TextXlsx, $"supervisor-report-{DateTime.Now:s}.xlsx");
        }
    }
}