using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Actors;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Extensions;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    [Route("Admin/PhotoPlatform/[controller]/[action]")]
    public class ActorController : BaseBaramjkPluginController
    {
        private readonly ActorAdminFactory _actorAdminFactory;
        private readonly ActorService _actorService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;


        public ActorController(
            ActorAdminFactory actorAdminFactory,
            ActorService actorService,
            INotificationService notificationService,
            ILocalizedEntityService localizedEntityService,
            ICustomerService customerService,
            ICustomerRegistrationService customerRegistrationService,
            IStoreContext storeContext,
            IPictureService pictureService)
        {
            _actorAdminFactory = actorAdminFactory;
            _actorService = actorService;
            _notificationService = notificationService;
            _localizedEntityService = localizedEntityService;
            _customerService = customerService;
            _customerRegistrationService = customerRegistrationService;
            _storeContext = storeContext;
            _pictureService = pictureService;
        }

        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Areas/Admin/Views/{FolderName}/{viewName}.cshtml";
        }

        private Task SubmitLocalizationsAsync(Actor actor, IList<ActorLocalizedModel> localized)
        {
            var tasks = localized.Select(model =>
                _localizedEntityService.SaveLocalizedValueAsync(actor, a => a.Name, model.Name, model.LanguageId));
            return Task.WhenAll(tasks);
        }

        [HttpGet]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public IActionResult List()
        {
            var searchModel = _actorAdminFactory.PrepareSearchModel();
            return View("List", searchModel);
        }

        [HttpPost]
        [AuthorizeDataTable(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ListAsync(ActorSearchModel searchModel)
        {
            var list = await _actorAdminFactory.PrepareListModelAsync(searchModel);
            return Json(list);
        }

        [HttpGet]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> CreateAsync()
        {
            var viewModel = await _actorAdminFactory.PrepareViewModelAsync();
            return View("Create", viewModel);
        }


        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateAsync(ActorViewModel viewModel, bool continueEditing)
        {
            if (string.IsNullOrEmpty(viewModel.Name))
            {
                if (viewModel.Locales.All(l => string.IsNullOrEmpty(l.Name)))
                {
                    _notificationService.ErrorNotification("Please enter a name.");
                    return RedirectToAction("Create");
                }

                viewModel.Name = viewModel
                    .Locales
                    .Last(l => string.IsNullOrEmpty(l.Name) == false)
                    .Name;
            }

            if (string.IsNullOrEmpty(viewModel.Email) || CommonHelper.IsValidEmail(viewModel.Email) == false)
            {
                _notificationService.ErrorNotification("Please enter a valid Email address.");
                return RedirectToAction("Create");
            }

            if (await _actorService.GetByEmailAsync(viewModel.Email) != null)
            {
                _notificationService.ErrorNotification("Actor already exists.");
                return RedirectToAction("Create");
            }


            if (string.IsNullOrWhiteSpace(viewModel.Password))
            {
                _notificationService.ErrorNotification("Please enter a valid password.");
                return RedirectToAction("Create");
            }


            var actorRole = await _customerService.GetCustomerRoleBySystemNameAsync(DefaultValues.ActorRoleName);
            var customer = await _customerService.GetCustomerByEmailAsync(viewModel.Email);


            if (customer == null)
            {
                customer = await _customerService.InsertGuestCustomerAsync();

                var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(
                    new CustomerRegistrationRequest(
                        customer,
                        viewModel.Email,
                        null,
                        viewModel.Password,
                        PasswordFormat.Hashed, (await _storeContext.GetCurrentStoreAsync()).Id));

                if (registrationResult.Success == false)
                {
                    _notificationService.ErrorNotification(string.Join(",", registrationResult.Errors));
                    return RedirectToAction("Create");
                }
            }


            if (await _customerService.IsInCustomerRoleAsync(customer, actorRole.SystemName) == false)
            {
                await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping
                    { CustomerId = customer.Id, CustomerRoleId = actorRole.Id });
            }


            await _customerService.UpdateCustomerAsync(customer);

            var actor = viewModel.Map<Actor>();

            actor.CustomerId = customer.Id;
            await _actorService.InsertAsync(actor);
            await SubmitLocalizationsAsync(actor, viewModel.Locales);
            return continueEditing
                ? RedirectToAction("Edit", new { id = actor.Id })
                : RedirectToAction("List");
        }

        [HttpGet("{id:int}")]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> EditAsync(int id)
        {
            var actor = await _actorService.GetByIdAsync(id);
            var viewModel = await _actorAdminFactory.PrepareViewModelAsync(actor);
            return View("Edit", viewModel);
        }

        [HttpPost("{id:int}")]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditAsync(int id, ActorViewModel viewModel, bool continueEditing)
        {
            var actor = await _actorService.GetByIdAsync(id);
            if (actor == null)
            {
                _notificationService.ErrorNotification("Actor not found.");
                return RedirectToAction("List");
            }


            actor.Name = viewModel.Name;
            actor.DefaultCustomerMobileEachPictureCost = viewModel.DefaultCustomerMobileEachPictureCost;
            actor.DefaultCameraManEachPictureCost = viewModel.DefaultCameraManEachPictureCost;
            actor.CardNumber = viewModel.CardNumber;
            actor.CardHolderName = viewModel.CardHolderName;

            await _actorService.UpdateAsync(actor);
            await SubmitLocalizationsAsync(actor, viewModel.Locales);

            return continueEditing
                ? RedirectToAction("Edit", new { id = actor.Id })
                : RedirectToAction("List");
        }

        [HttpPost]
        [AuthorizeContent(PermissionProvider.ManagementName)]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            var actor = await _actorService.GetByIdAsync(id);
            if (actor == null)
            {
                _notificationService.ErrorNotification("Actor not found.");
                return Content("Actor not found.");
            }

            var customerAssociated = await _customerService.GetCustomerByIdAsync(actor.CustomerId);
            var actorRole = await _customerService.GetCustomerRoleBySystemNameAsync(DefaultValues.ActorRoleName);

            await _customerService.RemoveCustomerRoleMappingAsync(customerAssociated, actorRole);

            await _actorService.DeleteAsync(actor);
            return new NullJsonResult();
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var actor = await _actorService.GetByIdAsync(id);
            if (actor == null)
            {
                _notificationService.ErrorNotification("Actor not found.");
                return RedirectToAction("List");
            }

            var customerAssociated = await _customerService.GetCustomerByIdAsync(actor.CustomerId);
            var actorRole = await _customerService.GetCustomerRoleBySystemNameAsync(DefaultValues.ActorRoleName);

            await _customerService.RemoveCustomerRoleMappingAsync(customerAssociated, actorRole);

            await _actorService.DeleteAsync(actor);
            return new NullJsonResult();
        }


        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ActorPictureDeleteAsync(int id)
        {
            var actorPicture = await _actorService.GetActorPictureByIdAsync(id);
            if (actorPicture == null)
            {
                _notificationService.ErrorNotification("Actor picture not found.");
                return Content("Actor picture not found.");
            }

            await _actorService.DeleteActorPictureAsync(actorPicture);
            return new NullJsonResult();
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ActorPictureEditAsync(ActorPictureItemModel model)
        {
            var actorPicture = await _actorService.GetActorPictureByIdAsync(model.Id);
            if (actorPicture == null)
            {
                _notificationService.ErrorNotification("Actor picture not found.");
                return Content("Actor picture not found.");
            }

            actorPicture.DisplayOrder = model.DisplayOrder;

            await _actorService.UpdateActorPictureAsync(actorPicture);
            return new NullJsonResult();
        }


        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ActorPictureListAsync(ActorPictureSearchModel searchModel)
        {
            var list = await _actorAdminFactory.PrepareActorPictureListModelAsync(searchModel);

            return Json(list);
        }


        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ActorPictureAddAsync(int actorId, int pictureId, int displayOrder)
        {
            if (pictureId == 0)
            {
                return Json(new { Result = false });
            }

            var actor = await _actorService.GetByIdAsync(actorId);
            if (actor == null)
            {
                _notificationService.ErrorNotification("Invalid Actor ID");
            }

            if ((await _actorService.GetActorPicturesByActorIdAsync(actorId)).Any(p => p.PictureId == pictureId))
                return Json(new { Result = false });

            var picture = await _pictureService.GetPictureByIdAsync(pictureId);
            if (picture == null)
            {
                _notificationService.ErrorNotification("No picture found with the specified id ");
                return Content("No picture found with the specified id ");
            }

            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType,
                picture.SeoFilename
            );


            await _actorService.InsertActorPictureAsync(new ActorPicture()
            {
                PictureId = pictureId,
                ActorId = actorId,
                DisplayOrder = displayOrder
            });

            return Json(new { Result = true });
        }
    }
}