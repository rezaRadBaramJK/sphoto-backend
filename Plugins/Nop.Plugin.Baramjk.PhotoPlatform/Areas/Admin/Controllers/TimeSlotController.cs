using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.TimeSlots;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Extensions;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [Route("Admin/PhotoPlatform/[controller]/[action]")]
    public class TimeSlotController : BaseBaramjkPluginController
    {
        private readonly TimeSlotAdminFactory _timeSlotAdminFactory;
        private readonly EventDetailsService _eventDetailsService;
        private readonly TimeSlotService _timeSlotService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizedEntityService _localizedEntityService;

        public TimeSlotController(
            TimeSlotAdminFactory timeSlotAdminFactory,
            EventDetailsService eventDetailsService,
            TimeSlotService timeSlotService,
            INotificationService notificationService,
            ILocalizedEntityService localizedEntityService)
        {
            _timeSlotAdminFactory = timeSlotAdminFactory;
            _eventDetailsService = eventDetailsService;
            _timeSlotService = timeSlotService;
            _notificationService = notificationService;
            _localizedEntityService = localizedEntityService;
        }


        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Areas/Admin/Views/{FolderName}/{viewName}.cshtml";
        }

        private async Task UpdateLocalizedAsync(TimeSlot timeSlot, IList<TimeSlotLocalizedModel> locales)
        {
            foreach (var local in locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(timeSlot, ts => ts.Note, local.Note,
                    local.LanguageId);
            }
        }

        [HttpPost]
        [AuthorizeDataTable(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ListAsync(EventTimeSlotsViewModel viewModel)
        {
            var model = await _timeSlotAdminFactory.PrepareTimeSlotListModelAsync(viewModel);
            return Json(model);
        }

        [HttpGet]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> CreateAsync([FromQuery] int eventId)
        {
            var viewModel = await _timeSlotAdminFactory.PrepareTimeSlotViewModel(eventId);
            return View("Create", viewModel);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateAsync([FromForm] TimeSlotViewModel viewModel, bool continueEditing)
        {
            var productEventDetail = await _eventDetailsService.GetProductEventDetailByProductIdAsync(viewModel.EventId);
            if (productEventDetail.Product == null)
            {
                _notificationService.ErrorNotification("Product not found.");
                return RedirectToAction("List", "Product");
            }

            if (productEventDetail.Product.Deleted)
            {
                _notificationService.ErrorNotification("Product is deleted.");
                return RedirectToAction("List", "Product");
            }

            var eventDetail = productEventDetail.EventDetails;

            if (eventDetail == null)
            {
                _notificationService.ErrorNotification("Event detail not found. Please set event details first.");
                return RedirectToAction("Create", "TimeSlot", new
                {
                    eventId = viewModel.EventId
                });
            }


            if (eventDetail.StartDate.Date > viewModel.Date ||
                eventDetail.EndDate.Date < viewModel.Date)
            {
                _notificationService.ErrorNotification("Invalid date range. Please select a date in event range.");
                return RedirectToAction("Create", "TimeSlot", new { eventId = viewModel.EventId });
            }


            if (eventDetail.StartTime > viewModel.StartTime ||
                eventDetail.EndTime < viewModel.EndTime)
            {
                _notificationService.ErrorNotification("Invalid time range. Please select time in event range.");
                return RedirectToAction("Create", "TimeSlot", new { eventId = viewModel.EventId });
            }

            var timeSlot = viewModel.Map<TimeSlot>();

            await _timeSlotService.InsertAsync(timeSlot);

            await UpdateLocalizedAsync(timeSlot, viewModel.Locales);

            return continueEditing
                ? RedirectToAction("EditData", "TimeSlot", new { eventId = viewModel.EventId })
                : RedirectToAction("Edit", "Product", new { id = viewModel.EventId });
        }

        [HttpGet("{id:int}")]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> EditAsync(int id)
        {
            var timeSlot = await _timeSlotService.GetByIdAsync(id);
            if (timeSlot == null)
            {
                _notificationService.ErrorNotification("TimeSlot not found.");
                return RedirectToAction("List", "Product");
            }

            var productEventDetails = await _eventDetailsService.GetProductEventDetailByProductIdAsync(timeSlot.EventId);
            if (productEventDetails == null)
            {
                return RedirectToAction("List", "Product");
            }

            if (productEventDetails.Product == null)
            {
                _notificationService.ErrorNotification("Product not found.");
                return RedirectToAction("List", "Product");
            }

            if (productEventDetails.Product.Deleted)
            {
                _notificationService.ErrorNotification("Product is deleted.");
                return RedirectToAction("List", "Product");
            }


            var viewModel = await _timeSlotAdminFactory.PrepareTimeSlotViewModel(timeSlot);
            return View("Edit", viewModel);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditDataAsync([FromForm] TimeSlotViewModel viewModel, bool continueEditing)
        {
            var databaseTimeSlot = await _timeSlotService.GetByIdAsync(viewModel.Id);
            if (databaseTimeSlot == null)
            {
                _notificationService.ErrorNotification("TimeSlot not found.");
                return RedirectToAction("Edit", "Product", new { id = viewModel.EventId });
            }

            var productEventDetail = await _eventDetailsService.GetProductEventDetailByProductIdAsync(viewModel.EventId);
            if (productEventDetail.Product == null)
            {
                _notificationService.ErrorNotification("Product not found.");
                return RedirectToAction("List", "Product");
            }

            if (productEventDetail.Product.Deleted)
            {
                _notificationService.ErrorNotification("Product is deleted.");
                return RedirectToAction("List", "Product");
            }

            var eventDetail = productEventDetail.EventDetails;

            if (eventDetail == null)
            {
                _notificationService.ErrorNotification("Event detail not found. Please set event details first.");
                return RedirectToAction("Create", "TimeSlot", new
                {
                    eventId = viewModel.EventId, viewModel
                });
            }


            if (eventDetail.StartDate.Date > viewModel.Date ||
                eventDetail.EndDate.Date < viewModel.Date)
            {
                _notificationService.ErrorNotification("Invalid date range. Please select a date in event range.");
                return RedirectToAction("Edit", "TimeSlot", new { id = viewModel.Id });
            }


            if (eventDetail.StartTime > viewModel.StartTime ||
                eventDetail.EndTime < viewModel.EndTime)
            {
                _notificationService.ErrorNotification("Invalid time range. Please select time in event range.");
                return RedirectToAction("Edit", "TimeSlot", new { id = viewModel.Id });
            }


            var timeSlotToUpdate = viewModel.Map<TimeSlot>();
            await _timeSlotService.UpdateAsync(timeSlotToUpdate);

            await UpdateLocalizedAsync(databaseTimeSlot, viewModel.Locales);

            return continueEditing
                ? RedirectToAction("Edit", "TimeSlot", new { id = viewModel.Id })
                : RedirectToAction("Edit", "Product", new { id = viewModel.EventId });
        }


        [HttpPost]
        [AuthorizeContent(PermissionProvider.ManagementName)]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            var timeSlot = await _timeSlotService.GetByIdAsync(id);
            if (timeSlot == null)
            {
                _notificationService.ErrorNotification("TimeSlot not found.");
                return Content("Time slot not found.");
            }

            await _timeSlotService.DeleteAsync(timeSlot);
            return new NullJsonResult();
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> Delete(int id)
        {
            var timeSlot = await _timeSlotService.GetByIdAsync(id);
            if (timeSlot == null)
            {
                _notificationService.ErrorNotification("TimeSlot not found.");
                return RedirectToAction("List", "Product");
            }

            await _timeSlotService.DeleteAsync(timeSlot);
            return RedirectToAction("Edit", "Product", new { id = timeSlot.EventId });
        }
    }
}