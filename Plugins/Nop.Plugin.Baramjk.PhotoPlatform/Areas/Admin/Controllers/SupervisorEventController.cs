using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.SupervisorEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [Route("Admin/PhotoPlatform/[controller]/[action]")]
    public class SupervisorEventController : BaseBaramjkPluginController
    {
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly SupervisorEventService _supervisorEventService;
        private readonly SupervisorEventAdminFactory _supervisorEventAdminFactory;

        public SupervisorEventController(ICustomerModelFactory customerModelFactory,
            SupervisorEventService supervisorEventService,
            SupervisorEventAdminFactory supervisorEventAdminFactory)
        {
            _customerModelFactory = customerModelFactory;
            _supervisorEventService = supervisorEventService;
            _supervisorEventAdminFactory = supervisorEventAdminFactory;
        }

        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Areas/Admin/Views/{FolderName}/{viewName}.cshtml";
        }

        [HttpGet]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> SupervisorAddPopupAsync()
        {
            var searchModel = await _customerModelFactory.PrepareCustomerSearchModelAsync(new CustomerSearchModel());
            return View("AddSupervisorPopup", searchModel);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ListAsync(EventSupervisorEventsViewModel viewModel)
        {
            var model = await _supervisorEventAdminFactory.PrepareEventSupervisorEventsListModelAsync(viewModel);
            return Json(model);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ListSupervisorsAsync(int eventId, CustomerSearchModel searchModel)
        {
            var model = await _supervisorEventAdminFactory.PrepareSupervisorListModelAsync(eventId, searchModel);
            return Json(model);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> AddEventSupervisorEventAsync([FromForm] AddSupervisorToSupervisorEventsModel model)
        {
            var previousRecords = await _supervisorEventService.GetEventSupervisorsAsync(model.EventId, model.SelectedCustomerIds.ToList());
            if (previousRecords.Any())
            {
                throw new NopException($"Supervisors with ids {string.Join(",", previousRecords.Select(ae => ae.CustomerId))} are already added.");
            }


            var supervisorEvents = model.SelectedCustomerIds.Select(customerId => new SupervisorEvent()
            {
                CustomerId = customerId,
                EventId = model.EventId,
                Active = true,
            }).ToList();


            await _supervisorEventService.InsertAsync(supervisorEvents);
            return Ok();
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            var record = await _supervisorEventService.GetByIdAsync(id);
            if (record == null)
            {
                throw new NopException("record not found");
            }

            await _supervisorEventService.DeleteAsync(record);
            return new NullJsonResult();
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> EditAsync(SupervisorEventsItemModel model)
        {
            var supervisorEvent = await _supervisorEventService.GetByIdAsync(model.Id);
            if (supervisorEvent == null)
            {
                return Content("SupervisorEvent not found");
            }

            supervisorEvent.Active = model.Active;

            await _supervisorEventService.UpdateAsync(supervisorEvent);

            return new NullJsonResult();
        }
    }
}