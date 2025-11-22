using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ActorEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Actors;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [Route("Admin/PhotoPlatform/[controller]/[action]")]
    public class ActorEventController : BaseBaramjkPluginController
    {
        private readonly ActorEventAdminFactory _actorEventAdminFactory;
        private readonly ActorService _actorService;
        private readonly ActorEventService _actorEventService;

        private readonly EventService _eventService;

        public ActorEventController(ActorEventAdminFactory actorEventAdminFactory,
            ActorService actorService,
            ActorEventService actorEventService,
            EventService eventService)
        {
            _actorEventAdminFactory = actorEventAdminFactory;
            _actorService = actorService;
            _actorEventService = actorEventService;
            _eventService = eventService;
        }

        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Areas/Admin/Views/{FolderName}/{viewName}.cshtml";
        }


        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ListAsync(EventActorEventsViewModel viewModel)
        {
            var model = await _actorEventAdminFactory.PrepareEventActorEventsListModelAsync(viewModel);
            return Json(model);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> AddEventActorEventAsync([FromForm] AddActorToActorEventsModel model)
        {
            var previousRecords = await _actorEventService.GetActorEventAsync(model.EventId, model.SelectedActorIds.ToList());
            if (previousRecords.Any())
            {
                throw new NopException($"Actors with ids {string.Join(",", previousRecords.Select(ae => ae.ActorId))} are already added.");
            }

            var eventDetails = await _eventService.GetEventDetailByEventId(model.EventId);

            if (eventDetails == null)
            {
                throw new NopException($"You should add some details for event first.");
            }


            var actors = await _actorService.GetByIdsAsync(model.SelectedActorIds.ToArray());
            if (actors == null)
            {
                throw new NopException("Actors not found");
            }


            var actorEvents = actors
                .Select(actor => new ActorEvent
                {
                    ActorId = actor.Id,
                    EventId = model.EventId,
                    CameraManEachPictureCost = eventDetails.PhotoPrice,
                    CustomerMobileEachPictureCost = eventDetails.PhotoPrice,
                    ProductionShare = eventDetails.ProductionShare,
                    ActorShare = eventDetails.ActorShare
                })
                .ToList();


            await _actorEventService.InsertAsync(actorEvents);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            var actorEvent = await _actorEventService.GetByIdAsync(id);
            if (actorEvent == null)
            {
                throw new NopException("ActorEvent not found");
            }

            await _actorEventService.DeleteAsync(actorEvent);
            return new NullJsonResult();
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> EditAsync(ActorEventsItemModel model)
        {
            var actorEvent = await _actorEventService.GetByIdAsync(model.Id);
            if (actorEvent == null)
            {
                return ErrorJson("ActorEvent not found");
            }

            var eventDetails = await _eventService.GetEventDetailByEventId(actorEvent.EventId);

            if (model.ActorShare < 0)
                return ErrorJson("Invalid amount, actor share must not be negative.");

            var newProductionShare = eventDetails.PhotoPrice - (eventDetails.PhotoShootShare + model.ActorShare);

            if (newProductionShare < 0)
                return ErrorJson("Invalid share provided. calculated production share must not be negative.");


            actorEvent.DisplayOrder = model.DisplayOrder;
            actorEvent.ActorShare = model.ActorShare;
            actorEvent.ProductionShare = newProductionShare;

            actorEvent.CameraManEachPictureCost = model.ActorPhotoPrice;
            actorEvent.CustomerMobileEachPictureCost = model.ActorPhotoPrice;

            await _actorEventService.UpdateAsync(actorEvent);

            return new NullJsonResult();
        }


        [HttpGet]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public IActionResult ActorAddPopup(int eventId)
        {
            var searchModel = _actorEventAdminFactory.PrepareAddActorViewModel(eventId);
            return View("AddActorPopup", searchModel);
        }


        [HttpGet]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public IActionResult BulkProductionShareEditPopUp(int eventId)
        {
            var model = _actorEventAdminFactory.PrepareBulkSharesEditViewModel(eventId);
            return View("BulkProductionShareEditPopUp", model);
        }

        [HttpGet]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public IActionResult BulkActorShareEditPopUp(int eventId)
        {
            var model = _actorEventAdminFactory.PrepareBulkSharesEditViewModel(eventId);
            return View("BulkActorShareEditPopUp", model);
        }


        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> BulkSharesEdit([FromForm] BulkSharesEditViewModel model)
        {
            if (model.EventId < 1)
                throw new NopException($"Invalid eventId");

            var allActorEvents = await _actorEventService.GetAllEventActorEventsAsync(model.EventId);

            if (model.ActorShare > 0)
                foreach (var ae in allActorEvents)
                {
                    ae.ActorShare = model.ActorShare;
                }


            if (model.ProductionShare > 0)
                foreach (var ae in allActorEvents)
                {
                    ae.ProductionShare = model.ProductionShare;
                }


            await _actorEventService.UpdateAsync(allActorEvents);
            return Ok();
        }


        [AuthorizeDataTable(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ListEventActorsAsync(int eventId, ActorSearchModel searchModel)
        {
            var list = await _actorEventAdminFactory.PrepareEventActorListModelAsync(eventId, searchModel);
            return Json(list);
        }
    }
}