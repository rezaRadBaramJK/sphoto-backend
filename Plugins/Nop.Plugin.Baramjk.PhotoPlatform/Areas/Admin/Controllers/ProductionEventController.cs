using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ProductionEvents;
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
    public class ProductionEventController : BaseBaramjkPluginController
    {
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ProductionEventService _productionEventService;
        private readonly ProductionEventAdminFactory _productionEventAdminFactory;

        public ProductionEventController(ICustomerModelFactory customerModelFactory,
            ProductionEventService productionEventService,
            ProductionEventAdminFactory productionEventAdminFactory)
        {
            _customerModelFactory = customerModelFactory;
            _productionEventService = productionEventService;
            _productionEventAdminFactory = productionEventAdminFactory;
        }

        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Areas/Admin/Views/{FolderName}/{viewName}.cshtml";
        }

        [HttpGet]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ProductionAddPopupAsync()
        {
            var searchModel = await _customerModelFactory.PrepareCustomerSearchModelAsync(new CustomerSearchModel());
            return View("AddProductionPopup", searchModel);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ListAsync(EventProductionEventsViewModel viewModel)
        {
            var model = await _productionEventAdminFactory.PrepareEventProductionEventsListModelAsync(viewModel);
            return Json(model);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ListProductionsAsync(int eventId, CustomerSearchModel searchModel)
        {
            var model = await _productionEventAdminFactory.PrepareProductionListModelAsync(eventId, searchModel);
            return Json(model);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> AddEventProductionEventAsync([FromForm] AddProductionToProductionEventsModel model)
        {
            var previousRecords = await _productionEventService.GetEventProductionsAsync(model.EventId, model.SelectedCustomerIds.ToList());
            if (previousRecords.Any())
            {
                throw new NopException($"Productions with ids {string.Join(",", previousRecords.Select(ae => ae.CustomerId))} are already added.");
            }


            var productionEvents = model.SelectedCustomerIds.Select(customerId => new ProductionEvent()
            {
                CustomerId = customerId,
                EventId = model.EventId,
                Active = true,
            }).ToList();


            await _productionEventService.InsertAsync(productionEvents);
            return Ok();
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            var record = await _productionEventService.GetByIdAsync(id);
            if (record == null)
            {
                throw new NopException("record not found");
            }

            await _productionEventService.DeleteAsync(record);
            return new NullJsonResult();
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> EditAsync(ProductionEventsItemModel model)
        {
            var productionEvent = await _productionEventService.GetByIdAsync(model.Id);
            if (productionEvent == null)
            {
                return Content("ProductionEvent not found");
            }

            productionEvent.Active = model.Active;

            await _productionEventService.UpdateAsync(productionEvent);

            return new NullJsonResult();
        }
    }
}