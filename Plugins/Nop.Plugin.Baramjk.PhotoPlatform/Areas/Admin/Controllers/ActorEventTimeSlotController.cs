using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ActorEventTimeSlots;
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
    public class ActorEventTimeSlotController : BaseBaramjkPluginController
    {
        private readonly ActorEventTimeSlotsAdminFactory _actorEventTimeSlotsAdminFactory;
        private readonly ActorEventTimeSlotService _actorEventTimeSlotService;

        public ActorEventTimeSlotController(ActorEventTimeSlotsAdminFactory actorEventTimeSlotsAdminFactory,
            ActorEventTimeSlotService actorEventTimeSlotService)
        {
            _actorEventTimeSlotsAdminFactory = actorEventTimeSlotsAdminFactory;
            _actorEventTimeSlotService = actorEventTimeSlotService;
        }


        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Areas/Admin/Views/{FolderName}/{viewName}.cshtml";
        }


        [HttpGet]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public IActionResult List([FromQuery] int timeSlotId, [FromQuery] DateTime timeSlotDate)
        {
            var searchModel = _actorEventTimeSlotsAdminFactory.PrepareSearchModel(timeSlotId,timeSlotDate);
            return View("List", searchModel);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ListAsync( ActorEventTimeSlotsSearchModel searchModel)
        {
 
            var model = await _actorEventTimeSlotsAdminFactory.PrepareListModelAsync(searchModel);
            return Json(model);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> EditAsync(ActorEventTimeSlotsItemModel model)
        {
            var entity = await _actorEventTimeSlotService.GetAsync(model.ActorEventId, model.TimeSlotId);


            if (entity != null)
            {
                entity.IsDeactivated = !model.Active;
                await _actorEventTimeSlotService.UpdateAsync(entity);
            }

            if (entity == null && model.Active == false)
            {
                entity = new ActorEventTimeSlot()
                {
                    ActorEventId = model.ActorEventId,
                    TimeSlotId = model.TimeSlotId,
                    IsDeactivated = !model.Active
                };
                await _actorEventTimeSlotService.InsertAsync(entity);
            }

            return new NullJsonResult();
        }
    }
}