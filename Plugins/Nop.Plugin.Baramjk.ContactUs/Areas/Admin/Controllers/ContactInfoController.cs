using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Factories;
using Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Models.ViewModels.ContactInfos;
using Nop.Plugin.Baramjk.ContactUs.Plugin;
using Nop.Plugin.Baramjk.ContactUs.Services;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Services.Directory;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Controllers
{
    [Permission(PermissionProvider.ManagementName)]
    [Area(AreaNames.Admin)]
    [Route("Admin/ContactUs/[controller]/[action]")]
    public class ContactInfoController: BaseBaramjkPluginController
    {
        private readonly ContactInfoAdminFactory _contactInfoAdminFactory;
        private readonly ContactInfoService _contactInfoService;
        private readonly INotificationService _notificationService;
        private readonly ICountryService _countryService;
        private readonly SubjectService _subjectService;
        private readonly IDownloadService _downloadService;

        public ContactInfoController(
            ContactInfoAdminFactory contactInfoAdminFactory,
            ContactInfoService contactInfoService, 
            INotificationService notificationService,
            ICountryService countryService,
            SubjectService subjectService,
            IDownloadService downloadService)
        {
            _contactInfoAdminFactory = contactInfoAdminFactory;
            _contactInfoService = contactInfoService;
            _notificationService = notificationService;
            _countryService = countryService;
            _subjectService = subjectService;
            _downloadService = downloadService;
        }

        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Areas/Admin/Views/{ControllerName}/{viewName}";
        }

        [HttpGet]
        public IActionResult ListAsync()
        {
            var searchModel = _contactInfoAdminFactory.PrepareSearchModel();
            return View("List.cshtml", searchModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> ListAsync(ContactInfoSearchModel searchModel)
        {
            var model = await _contactInfoAdminFactory.PrepareListModelAsync(searchModel);
            return Json(model);
        }
        
        
        [HttpGet("{id:int}")]
        public async Task<IActionResult> EditAsync(int id)
        {
            var entity = await _contactInfoService.GetByIdAsync(id);
            if (entity == null)
                return RedirectToAction("List");

            var model = await _contactInfoAdminFactory.PrepareViewModelAsync(entity);
            return View("Edit.cshtml", model);
        }

        [HttpPost("{id:int}"), ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditAsync(int id, ContactInfoViewModel viewModel, bool continueEditing)
        {
            if (id != viewModel.Id)
                return RedirectToAction("List");
            
            var entity = await _contactInfoService.GetByIdAsync(viewModel.Id);
            if (entity == null)
            {
                _notificationService.ErrorNotification("Invalid contact info. Contact info not found.");
                return RedirectToAction("List");
            }
            
            if (string.IsNullOrEmpty(viewModel.FirstName))
            {
                _notificationService.ErrorNotification("Invalid first name.");
                return View("Edit.cshtml", viewModel);
            }
            
            if (string.IsNullOrEmpty(viewModel.LastName))
            {
                _notificationService.ErrorNotification("Invalid last name.");
                return View("Edit.cshtml", viewModel);
            }
            
            if (string.IsNullOrEmpty(viewModel.Email) || CommonHelper.IsValidEmail(viewModel.Email) == false)
            {
                _notificationService.ErrorNotification("Invalid email.");
                return View("Edit.cshtml", viewModel);
            }

            var country = await _countryService.GetCountryByIdAsync(viewModel.CountryId);
            if (country == null)
            {
                _notificationService.ErrorNotification("Invalid country. Country not found.");
                return View("Edit.cshtml", viewModel);
            }

            var subject = await _subjectService.GetByIdAsync(viewModel.SubjectId);
            if (subject == null)
            {
                _notificationService.ErrorNotification("Invalid subject. Subject not found.");
                return View("Edit.cshtml", viewModel);
            }

            if (viewModel.FileId != 0)
            {
                var download = await _downloadService.GetDownloadByIdAsync(viewModel.FileId);
                if (download == null)
                {
                    _notificationService.ErrorNotification("Invalid file. File not found.");
                    return View("Edit.cshtml", viewModel);
                }
            }

            entity.FirstName = viewModel.FirstName;
            entity.LastName = viewModel.LastName;
            entity.CountryId = viewModel.CountryId;
            entity.PhoneNumber = viewModel.PhoneNumber;
            entity.Email = viewModel.Email;
            entity.SubjectId = viewModel.SubjectId;
            entity.FileId = viewModel.FileId;
            entity.Message = viewModel.Message;
            if (entity.HasBeenPaid && viewModel.HasBeenPaid == false)
            {
                entity.HasBeenPaid = false;
                entity.PaymentUtcDateTime = null;
            }
            else if(entity.HasBeenPaid == false && viewModel.HasBeenPaid)
            {
                entity.HasBeenPaid = true;
                entity.PaymentUtcDateTime = DateTime.UtcNow;
            }
            
            await _contactInfoService.UpdateAsync(entity);
            
            _notificationService.SuccessNotification("Contact info has been saved.");
            
            return continueEditing
                ? RedirectToAction("Edit", new { id = entity.Id })
                : RedirectToAction("List");
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteById(int id)
        {
            await _contactInfoService.DeleteByIdAsync(id);
            return new NullJsonResult(); 
        }
    }
}