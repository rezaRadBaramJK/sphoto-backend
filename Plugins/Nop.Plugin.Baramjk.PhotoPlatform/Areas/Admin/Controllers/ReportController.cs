using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Cashier;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.EventCashier;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Tickets;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    [Route("Admin/PhotoPlatform/[controller]/[action]")]
    public class ReportController : BaseBaramjkPluginController
    {
        private readonly EventService _eventService;
        private readonly ReportAdminFactory _reportAdminFactory;
        private readonly TimeSlotService _timeSlotService;
        private readonly ReportsService _reportsService;
        private readonly INotificationService _notificationService;
        private readonly ExportService _exportService;
        private readonly CashierEventService _cashierEventService;


        public ReportController(EventService eventService,
            ReportAdminFactory reportAdminFactory,
            TimeSlotService timeSlotService,
            INotificationService notificationService,
            ReportsService reportsService,
            ExportService exportService, CashierEventService cashierEventService)
        {
            _eventService = eventService;
            _reportAdminFactory = reportAdminFactory;
            _timeSlotService = timeSlotService;
            _notificationService = notificationService;
            _reportsService = reportsService;
            _exportService = exportService;
            _cashierEventService = cashierEventService;
        }


        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Areas/Admin/Views/{FolderName}/{viewName}.cshtml";
        }

        private string GetPdfViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Areas/Admin/Views/Reports/PdfStructures/{viewName}.cshtml";
        }

        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> CashierReportAsync()
        {
            var events = await _eventService.GetEventsBriefDataAsync();


            var model = new CashierReportViewModel
            {
                AvailableEvents = _reportAdminFactory.PrepareEvents(events)
            };

            return View("CashierReport", model);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> GenerateCashierReportAsync(CashierReportViewModel model)
        {
            if (model.EventId == 0)
            {
                _notificationService.ErrorNotification("Invalid EventId");
                return RedirectToAction("CashierReport");
            }

            var items = await _reportsService.GetEventTimeSlotReservationsAsync(model.EventId, model.TimeSlotId);

            if (items.Any() == false)
            {
                _notificationService.ErrorNotification("There's not any reservation with selected data");
                return RedirectToAction("CashierReport");
            }

            var preparedItems = await _reportAdminFactory.PrepareCashierReportPdfData(items, model.ShowTicketsCount);

            if (preparedItems.Reservations.Any() == false)
            {
                _notificationService.ErrorNotification("There's not any confirmed photo shoot with selected data");
                return RedirectToAction("CashierReport");
            }

            return new ViewAsPdf(GetPdfViewPath("CashierReportPDF"), preparedItems)
            {
                ContentDisposition = ContentDisposition.Attachment,
                FileName = $"Report-{DateTime.Now:s}.pdf",
                PageWidth = 80,
                PageHeight = 297,
                PageMargins = new Margins(5, 5, 5, 5)
            };
        }

        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> GetDatesByEventAsync(int eventId)
        {
            var eventDetails = await _eventService.GetEventDetailByEventId(eventId);
            var preparedItems = _reportAdminFactory.PrepareEventDates(eventDetails);

            return Json(preparedItems);
        }


        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> GetDatesByEventsAsync([FromQuery] List<int> eventIds)
        {
            if (eventIds.Any() == false) throw new ArgumentNullException();
            var eventDetails = await _eventService.GetEventDetailByEventIds(eventIds);
            var preparedItems = _reportAdminFactory.PrepareMinAndMaxEventDates(eventDetails);

            return Json(preparedItems);
        }

        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> GetEventTimeSlotsByEventAndDate(int eventId, DateTime date)
        {
            var timeSlots = await _timeSlotService.GetEventTimeSlotsAsync(eventId, date);
            var preparedItems = _reportAdminFactory.PrepareEventTimeSlots(timeSlots);

            return Json(preparedItems);
        }

        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> GetEventTimeSlotsByEventAndDateForCashierReport(int eventId, DateTime date)
        {
            var timeSlots = await _timeSlotService.GetEventTimeSlotsAsync(eventId, date);
            var preparedItems = _reportAdminFactory.PrepareEventTimeSlotsForCashier(timeSlots);

            return Json(preparedItems);
        }

        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> GetEventTimeSlotsByDate(int[] eventIds, DateTime? fromDate, DateTime? toDate)
        {
            var timeSlots = await _timeSlotService.GetEventsTimeSlotsAsync(eventIds, fromDate, toDate);
            var preparedItems = _reportAdminFactory.PrepareEventTimeSlots(timeSlots);

            return Json(preparedItems);
        }


        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> SalesReportAsync()
        {
            var events = await _eventService.GetEventsBriefDataAsync();


            var model = new SalesReportViewModel
            {
                AvailableEvents = events.Select(e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id.ToString()
                }).ToList(),
            };
            model.AvailableEvents.Insert(0, new SelectListItem
            {
                Text = "--",
                Value = "0"
            });
            return View("SalesReport", model);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> GenerateSalesReportAsync(SalesReportViewModel model)
        {
            if (model.EventId == 0)
            {
                _notificationService.ErrorNotification("Invalid EventId");
                return RedirectToAction("SalesReport");
            }

            var reservations = await _reportsService.GetAllEventReservationsAsync(model.EventId);
            if (reservations.Any() == false)
            {
                _notificationService.ErrorNotification("No Reservations were found");
                return RedirectToAction("SalesReport");
            }

            var preparedItems = _reportAdminFactory.PrepareSalesReportPdfData(reservations, model.OnlyShowConfirmedTickets);

            var relativePath = Url.Action(
                action: "SalesPdfFooter",
                controller: "Report");

            var footerUrl = $"{Request.Scheme}://{Request.Host}{relativePath}";
            var switches =
                $"--footer-html \"{footerUrl}\" " +
                "--footer-spacing 10";
            return new ViewAsPdf(GetPdfViewPath("SalesReportPDF"), preparedItems)
            {
                ContentDisposition = ContentDisposition.Attachment,
                FileName = $"SalesReport-{DateTime.Now:s}.pdf",
                PageSize = Size.A4,
                CustomSwitches = switches,
                PageMargins = new Margins(5, 5, 20, 5)
            };
        }

        [AllowAnonymous]
        public IActionResult SalesPdfFooter(int page, int topage)
        {
            ViewBag.Page = page;
            ViewBag.ToPage = topage;
            ViewBag.DateStr = DateTime.Now.ToString("dddd dd MMMM yyyy");
            return View("PdfStructures/SalesReportPDFFooter");
        }


        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> GetEventCashiersAsync(int eventId)
        {
            var eventDetails = await _eventService.GetEventCashiersDetails(eventId);
            var preparedItems = _reportAdminFactory.PrepareEventCashiersData(eventDetails);

            return Json(preparedItems);
        }

        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> GetCashierEvents(int cashierId)
        {
            var cashierEvents = await _eventService.GetCashierEventsAsync(cashierId);
            var preparedItems = _reportAdminFactory.PrepareEvents(cashierEvents.Select(e => e.Product).ToList());

            return Json(preparedItems);
        }


        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> CashierEventReportAsync()
        {
            var events = await _eventService.GetEventsBriefDataAsync();

            var model = new CashierEventReportViewModel
            {
                AvailableEvents = _reportAdminFactory.PrepareEvents(events),
            };

            return View("CashierEventReport", model);
        }


        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> GenerateCashierEventReportAsync(CashierEventReportViewModel model)
        {
            var cashierReservations =
                await _reportsService.GetCashierEventReservationsForCashierReportAsync(model.EventId, model.CashierId, model.EventDate,
                    model.TimeSlotId);

            var onlineReservations =
                await _reportsService.GetOnlineEventReservationsForCashierReportAsync(model.EventId, model.CashierId, model.EventDate,
                    model.TimeSlotId);

            if (cashierReservations.Any() == false && onlineReservations.Any() == false)
            {
                _notificationService.ErrorNotification("No Reservations were found");
                return RedirectToAction("CashierEventReport");
            }

            var includeTimeSlot = model.TimeSlotId != null;

            var cashierEvent = await _cashierEventService.GetByCashierIdAndEventIdAsync(model.CashierId, model.EventId);

            var cashierDayBalance = await _cashierEventService.GetCashierDailyBalanceAsync(cashierEvent.Id, model.EventDate);

            var data = await _reportAdminFactory.PrepareCashierEventPdfData(cashierReservations, onlineReservations, includeTimeSlot,
                cashierDayBalance?.OpeningFundBalanceAmount??0);

            return new ViewAsPdf(GetPdfViewPath("CashierEventReportPDF"), data)
            {
                ContentDisposition = ContentDisposition.Attachment,
                FileName = $"CashierEventReport-{DateTime.Now:s}.pdf",
                PageSize = Size.A7,
                PageMargins = new Margins(2, 2, 2, 2)
            };
        }


        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> TicketsReport()
        {
            var events = await _eventService.GetEventsBriefDataAsync();

            var model = new TicketsReportViewModel()
            {
                AvailableEvents = _reportAdminFactory.PrepareEvents(events),
            };

            return View("TicketsReport", model);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> GenerateTicketsReportAsync(TicketsReportViewModel model)
        {
            var data = await _reportsService.GetTicketReportDetailsAsync(model.EventIds.ToList(), model.FromDate, model.ToDate,
                model.TimeSlotStartTime);

            var result = await _reportAdminFactory.PrepareTicketsReportExportModelAsync(data);

            var bytes = await _exportService.ExportTicketsRevenueAsync(result);

            return File(bytes, MimeTypes.TextXlsx, $"tickets-report-{DateTime.Now:s}.xlsx");
        }
    }
}