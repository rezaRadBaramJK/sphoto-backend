using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Factories;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models.Suppliers;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Controllers
{
    [Permission(PermissionProvider.MyFatoorahManagement)]
    [Area(AreaNames.Admin)]
    [Route("Admin/MyFatoorah/Supplier/[action]")]
    public class SupplierAdminController : BaseBaramjkPluginController
    {
        private readonly SupplierAdminFactory _supplierFactory;
        private readonly SupplierService _supplierService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IProductService _productService;
        private readonly MyFatoorahSettings _myFatoorahSettings;

        public SupplierAdminController(
            SupplierAdminFactory supplierFactory,
            SupplierService supplierService,
            ILocalizedEntityService localizedEntityService,
            INotificationService notificationService, 
            IProductService productService,
            MyFatoorahSettings myFatoorahSettings)
        {
            _supplierFactory = supplierFactory;
            _supplierService = supplierService;
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _productService = productService;
            _myFatoorahSettings = myFatoorahSettings;
        }

        protected override string GetViewPath(string viewName) =>
            $"~/Plugins/{SystemName}/Views/Suppliers/{viewName}";

        public async Task SubmitLocalizationsAsync(Supplier supplier, IList<SupplierLocalizationModel> localizations)
        {
            foreach (var model in localizations)
            {
                await _localizedEntityService
                    .SaveLocalizedValueAsync(
                        supplier, 
                        b => b.Name, 
                        model.Name, 
                        model.LanguageId);
            }
        }

        [HttpGet]
        public IActionResult List()
        {
            var searchModel = new SupplierSearchModel();
            searchModel.SetGridPageSize();

            return View("List.cshtml", searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> ListAsync(SupplierSearchModel searchModel)
        {
            var listModel = await _supplierFactory.PrepareListModelAsync(searchModel);
            return Json(listModel);
        }
        
        [HttpGet("{id:int}")]
        public async Task<IActionResult> EditAsync(int id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);
            if (supplier == null)
            {
                _notificationService.ErrorNotification("Supplier not found.");
                return RedirectToAction("List");
            }
            var viewModel = await _supplierFactory.PrepareViewModelAsync(supplier);
            return View("Edit.cshtml", viewModel);
        }
        
        [HttpPost("{id:int}"), ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditAsync(int id, [FromForm] SupplierViewModel viewModel, bool continueEditing)
        {
            if (id != viewModel.Id)
                return RedirectToAction("List");

            if (viewModel.SupplierCode <= 0)
            {
                _notificationService.ErrorNotification("invalid supplier code.");
                return RedirectToAction("Edit", new { id });
            }
            
            var supplier = await _supplierService.GetByIdAsync(viewModel.Id);
            if (supplier == null)
            {
                _notificationService.ErrorNotification("Supplier not found.");
                return RedirectToAction("List");
            }

            supplier.Name = viewModel.Name;
            
            await _supplierService.UpdateAsync(supplier);
            await SubmitLocalizationsAsync(supplier, viewModel.Locales);
            
            return continueEditing
                ? RedirectToAction("Edit", new { id = viewModel.Id })
                : RedirectToAction("List");
        }
        
        [HttpPost]
        public async Task<IActionResult> AssignProductAsync(ProductSupplierViewModel viewModel)
        {
            if(viewModel.SupplierId <= 0)
                return BadRequest("Invalid supplier id.");
            
            if(viewModel.ProductId <= 0)
                return BadRequest("Invalid product id.");

            var supplier =  await _supplierService.GetByIdAsync(viewModel.SupplierId);
            if(supplier == null)
                return BadRequest("Supplier not found.");

            var product = await _productService.GetProductByIdAsync(viewModel.ProductId);
            if(product == null)
                return BadRequest("Product not found.");
            
            await _supplierService.AssignProductAsync(viewModel.ProductId, viewModel.SupplierId);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> SyncAsync()
        {
            if (_myFatoorahSettings.MyFatoorahUseSandbox)
            {
                _notificationService.ErrorNotification("Syncing suppliers is not allowed in sandbox mode.");
                return RedirectToAction("List");
            }
            
            await _supplierService.SyncSuppliersAsync();
            _notificationService.SuccessNotification("Suppliers synced successfully.");
            return RedirectToAction("List");
        }
    }
}