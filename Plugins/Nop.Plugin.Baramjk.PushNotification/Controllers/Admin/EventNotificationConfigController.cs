using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.PushNotification.Domain;
using Nop.Plugin.Baramjk.PushNotification.Factories.Admin;
using Nop.Plugin.Baramjk.PushNotification.Models.Admin.EventNotificationConfigs;
using Nop.Plugin.Baramjk.PushNotification.Plugins;
using Nop.Plugin.Baramjk.PushNotification.Services;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.PushNotification.Controllers.Admin
{
    [Permission(PermissionProvider.PushNotificationManagementName)]
    [Area(AreaNames.Admin)]
    public class EventNotificationConfigController : BaseBaramjkPluginController
    {
        private readonly EventNotificationConfigService _eventNotificationConfigService;
        private readonly EventNotificationConfigFactory _eventNotificationConfigFactory;
        private readonly INotificationService _notificationService;

        public EventNotificationConfigController(
            EventNotificationConfigService eventNotificationConfigService,
            EventNotificationConfigFactory eventNotificationConfigFactory,
            INotificationService notificationService)
        {
            _eventNotificationConfigService = eventNotificationConfigService;
            _eventNotificationConfigFactory = eventNotificationConfigFactory;
            _notificationService = notificationService;
        }

        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Views/Menu/EventNotificationConfigs/{viewName}";
        }

        public virtual IActionResult Index()
        {
            return RedirectToAction("Events");
        }

        [HttpPost]
        public async Task<IActionResult> ListAsync(EventNotificationConfigSearchModel searchModel)
        {
            var model = await _eventNotificationConfigFactory.EventNotificationConfigListModelAsync(searchModel);
            return Json(model);
        }

        public IActionResult EventsAsync()
        {
            var model = _eventNotificationConfigFactory.PrepareSearchModel();
            return View("List.cshtml", model);
        }

        public async Task<IActionResult> CreateAsync()
        {
            var  model = await _eventNotificationConfigFactory.PrepareModelAsync();
            return View("Create.cshtml", model);
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateAsync(EventNotificationConfigModel model, bool continueEditing)
        {
            var entity = await _eventNotificationConfigService.GetAsync(model.EventName, model.StatusName);
            if (entity == null)
            {
                var errorMessage =
                    await _localizationService.GetResourceAsync(
                        $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.AlreadyExists");
                _notificationService.ErrorNotification(errorMessage);
                return RedirectToAction("Create", model);
            }

            entity = new EventNotificationConfigEntity
            {
                EventName = model.EventName,
                StatusName = model.EventName != DefaultValue.OrderNoteInsertedEventKey 
                    ? model.StatusName 
                    : string.Empty,
                TemplateName = model.TemplateName,
                UseFirebase = model.UseFirebase,
                UseSms = model.UseSms,
                UseWhatsApp = model.UseWhatsApp
            };

            await _eventNotificationConfigService.InsertAsync(entity);

            return continueEditing
                ? RedirectToAction("Edit", new { id = entity.Id })
                : RedirectToAction("Events");
        }

        [HttpGet]
        public async Task<IActionResult> EditAsync(int id)
        {
            var entity = await _eventNotificationConfigService.GetByIdAsync(id);
            if (entity == null)
                return RedirectToAction("Events");

            var model = await _eventNotificationConfigFactory.PrepareModelAsync(entity);
            return View("Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditAsync(EventNotificationConfigModel model, bool continueEditing)
        {
            var entity = await _eventNotificationConfigService.GetByIdAsync(model.Id);
            if (entity == null)
                return RedirectToAction("Events");

            entity.EventName = model.EventName;
            entity.StatusName = model.EventName != DefaultValue.OrderNoteInsertedEventKey 
                ? model.StatusName 
                : string.Empty;
            entity.UseFirebase = model.UseFirebase;
            entity.TemplateName = model.TemplateName;
            entity.UseSms = model.UseSms;
            entity.UseWhatsApp = model.UseWhatsApp;

            await _eventNotificationConfigService.UpdateAsync(entity);
            return continueEditing
                ? RedirectToAction("Edit", new { id = entity.Id })
                : RedirectToAction("Events");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _eventNotificationConfigService.GetByIdAsync(id);
            if (entity != null)
                await _eventNotificationConfigService.DeleteAsync(entity);

            return RedirectToAction("Events");
        }

        [HttpGet]
        public async Task<IActionResult> GetStatusByEventNameAsync(string eventName, int configId)
        {
            var statusNames =
                await _eventNotificationConfigFactory.PrepareStatusNameListItemsByEventNameAsync(
                    eventName,
                    configId);
            
            var result = statusNames.Select(sn => new { Id = sn.Value, Name = sn.Text, sn.Selected }).ToList();
            return ApiResponseFactory.Success(result);
        }
    }
}