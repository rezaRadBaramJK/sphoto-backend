using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Subjects;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services.ContactUs;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Controllers
{
    [Permission(PermissionProvider.ManagementName)]
    [Area(AreaNames.Admin)]
    [Route("Admin/PhotoPlatform/[controller]/[action]")]
    public class ContactUsSubjectController : BaseBaramjkPluginController
    {
        private readonly SubjectAdminFactory _subjectAdminFactory;
        private readonly PhotoPlatformSubjectService _subjectService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizedEntityService _localizedEntityService;

        public ContactUsSubjectController(
            SubjectAdminFactory subjectAdminFactory,
            PhotoPlatformSubjectService subjectService,
            INotificationService notificationService,
            ILocalizedEntityService localizedEntityService)
        {
            _subjectAdminFactory = subjectAdminFactory;
            _subjectService = subjectService;
            _notificationService = notificationService;
            _localizedEntityService = localizedEntityService;
        }

        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Areas/Admin/Views/ContactUs/{ControllerName}/{viewName}";
        }

        private async Task UpdateLocalizedAsync(SubjectViewModel viewModel, SubjectEntity entity)
        {
            foreach (var localized in viewModel.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(entity,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            }
        }


        [HttpGet]
        public IActionResult ListAsync()
        {
            var searchModel = _subjectAdminFactory.PrepareSearchModel();
            return View("List.cshtml", searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> ListAsync(SubjectSearchModel searchModel)
        {
            var model = await _subjectAdminFactory.PrepareListModelAsync(searchModel);
            return Json(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateAsync()
        {
            var viewModel = await _subjectAdminFactory.PrepareViewModelAsync();
            return View("Create.cshtml", viewModel);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateAsync(SubjectViewModel viewModel, bool continueEditing)
        {
            if (string.IsNullOrEmpty(viewModel.Name))
            {
                _notificationService.ErrorNotification("Invalid name.");
                return View("Create.cshtml", viewModel);
            }

            var entity = new SubjectEntity
            {
                Name = viewModel.Name,
            };

            await _subjectService.InsertAsync(entity);
            await UpdateLocalizedAsync(viewModel, entity);

            return continueEditing
                ? RedirectToAction("Edit", new { id = entity.Id })
                : RedirectToAction("List");
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> EditAsync(int id)
        {
            var entity = await _subjectService.GetByIdAsync(id);
            if (entity == null)
                return RedirectToAction("List");

            var model = await _subjectAdminFactory.PrepareViewModelAsync(entity);
            return View("Edit.cshtml", model);
        }

        [HttpPost("{id:int}"), ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditAsync(int id, SubjectViewModel viewModel, bool continueEditing)
        {
            if (id != viewModel.Id)
                return RedirectToAction("List");

            if (string.IsNullOrEmpty(viewModel.Name))
            {
                _notificationService.ErrorNotification("Invalid name.");
                return View("Edit.cshtml", viewModel);
            }

            var entity = await _subjectService.GetByIdAsync(viewModel.Id);
            if (entity == null)
            {
                _notificationService.ErrorNotification("Subject not found.");
                return RedirectToAction("List");
            }

            entity.Name = viewModel.Name;


            await _subjectService.UpdateAsync(entity);

            await UpdateLocalizedAsync(viewModel, entity);

            _notificationService.SuccessNotification("Subject has been saved.");

            return continueEditing
                ? RedirectToAction("Edit", new { id = entity.Id })
                : RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            await _subjectService.DeleteAsync(id);
            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _subjectService.DeleteAsync(id);
            return RedirectToAction("List");
        }
    }
}