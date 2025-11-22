using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.Wallet.Domain;
using Nop.Plugin.Baramjk.Wallet.Factories;
using Nop.Plugin.Baramjk.Wallet.Models.ViewModels.Packages;
using Nop.Plugin.Baramjk.Wallet.Plugins;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Services.Directory;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.Wallet.Controllers
{
    [Permission(PermissionProvider.Wallet_MANAGEMEN)]
    [Area(AreaNames.Admin)]
    public class PackageController : BaseBaramjkPluginController
    {
        protected override string SystemName => DefaultValue.SystemName;

        private readonly WalletPackageService _walletPackageService;
        private readonly PackageAdminFactory _packageAdminFactory;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ICurrencyService _currencyService;

        public PackageController(
            WalletPackageService walletPackageService,
            PackageAdminFactory packageAdminFactory,
            IPermissionService permissionService, INotificationService notificationService,
            ICurrencyService currencyService)
        {
            _walletPackageService = walletPackageService;
            _packageAdminFactory = packageAdminFactory;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _currencyService = currencyService;
        }

        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Views/{ControllerName}/{viewName}.cshtml";
        }

        #region Package actions

        [HttpGet]
        public async Task<IActionResult> ListAsync()
        {
            if (await _permissionService.AuthorizeAsync(PermissionProvider.Wallet_MANAGEMEN) == false)
                return AccessDeniedView();

            var searchModel = _packageAdminFactory.PrepareSearchModel();
            return View("List", searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> ListAsync(PackageSearchModel searchModel)
        {
            var list = await _packageAdminFactory.PrepareListModelAsync(searchModel);
            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> AddOrEditAsync(int? id)
        {
            AddOrEditPackageViewModel model = null;
            if (id.HasValue)
            {
                model = await _packageAdminFactory.PrepareEditModelAsync(id.Value);
                if (model is null)
                    return NotFound();
            }
            else
                model = await _packageAdminFactory.PrepareAddModelAsync();

            return View("AddOrEdit", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEditAsync(AddOrEditPackageViewModel model)
        {
            var package = await _walletPackageService.AddOrEditPackageAsync(model);
            if (package is null)
                return NotFound();
            return RedirectToAction("List");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _walletPackageService.DeletePackageAsync(id);
            return Json(new { });
        }

        #endregion

        #region Package item actions

        [HttpGet]
        public async Task<IActionResult> AddOrEditItemAsync(int? id, int? packageId)
        {
            AddOrEditPackageItemViewModel model = null;
            if (id.HasValue)
            {
                model = await _packageAdminFactory.PreparePackageItemEditModelAsync(id.Value);
                if (model is null)
                    return NotFound();
            }
            else
            {
                if (!packageId.HasValue)
                {
                    _notificationService.ErrorNotification("Package is invalid!");
                    return RedirectToAction("List");
                }
                else
                {
                    var package = await _walletPackageService.GetByIdAsync(packageId.Value);
                    if (package is null)
                    {
                        _notificationService.ErrorNotification("Package not founded!");
                        return RedirectToAction("List");
                    }

                    model = new AddOrEditPackageItemViewModel()
                    {
                        PackageId = package.Id,
                    };
                }
            }

            return View("AddOrEditItem", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEditItemAsync(AddOrEditPackageItemViewModel model)
        {
            var currency = await _currencyService.GetCurrencyByCodeAsync(model.CurrencyCode);
            if (currency is null)
            {
                ModelState.AddModelError(nameof(model.CurrencyCode), "Currency is invalid!");
                return View("AddOrEditItem", model);
            }

            var package = await _walletPackageService.GetByIdAsync(model.PackageId);
            if (package is null)
            {
                _notificationService.ErrorNotification("Package is invalid!");
                return RedirectToAction("List");
            }

            var packageItem = new WalletItemPackage()
            {
                WalletPackageId = package.Id,
                CurrencyCode = currency.CurrencyCode,
                Amount = model.Amount,
                Id = model.Id.HasValue ? model.Id.Value : 0,
            };
            if (model.Id > 0)
            {
                if (await _walletPackageService.IsDuplicatePackageItemByCurrencyCodeAsync(model.Id,package.Id,currency.CurrencyCode))
                {
                    ModelState.AddModelError(nameof(model.CurrencyCode), "Currency is duplicated!");
                    return View("AddOrEditItem", model); 
                }
                var result = await _walletPackageService.EditPackageItemAsync(packageItem);
                if (result is null)
                    return NotFound();
            }
            else
            {
                if (await _walletPackageService.IsDuplicatePackageItemByCurrencyCodeAsync(null,package.Id,currency.CurrencyCode))
                {
                    ModelState.AddModelError(nameof(model.CurrencyCode), "Currency is duplicated!");
                    return View("AddOrEditItem", model); 
                }
                var result = await _walletPackageService.AddPackageItemAsync(packageItem);
            }

            return RedirectToAction("List");
        }

        
        [HttpGet]
        public async Task<IActionResult> DeletePackageItem(int id)
        {
            await _walletPackageService.DeletePackageItemAsync(id);
            return Json(new { });
        }

        #endregion
    }
}