using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.CashierEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains.Types;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.CashierBalanceHistory;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Messages;
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
    public class CashierEventController : BaseBaramjkPluginController
    {
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly CashierEventService _cashierEventService;
        private readonly CashierEventAdminFactory _cashierEventAdminFactory;
        private readonly CashierBalanceService _cashierBalanceService;
        private readonly INotificationService _notificationService;

        public CashierEventController(ICustomerModelFactory customerModelFactory,
            CashierEventService cashierEventService,
            CashierEventAdminFactory cashierEventAdminFactory,
            CashierBalanceService cashierBalanceService, INotificationService notificationService)
        {
            _customerModelFactory = customerModelFactory;
            _cashierEventService = cashierEventService;
            _cashierEventAdminFactory = cashierEventAdminFactory;
            _cashierBalanceService = cashierBalanceService;
            _notificationService = notificationService;
        }


        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Areas/Admin/Views/{FolderName}/{viewName}.cshtml";
        }

        [HttpGet]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> CashierAddPopupAsync()
        {
            var searchModel = await _customerModelFactory.PrepareCustomerSearchModelAsync(new CustomerSearchModel());
            return View("AddCashierPopup", searchModel);
        }

        [HttpGet]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> EditCashierDailyBalancePopup(int eventId)
        {
            var model = await _cashierEventAdminFactory.PrepareEventCashierDailyBalanceViewModel(eventId);
            return View("EditCashierDailyBalancePopup", model);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> EditCashierDailyBalancePopupAsync(CashierDailyBalanceViewModel model)
        {
            var cashierEvent = await _cashierEventService.GetByCashierIdAndEventIdAsync(model.CashierId, model.EventId);
            if (cashierEvent == null)
            {
                _notificationService.ErrorNotification("Cashier or event is not valid ");

                return RedirectToAction("EditCashierDailyBalancePopup");
            }

            var entity = await _cashierEventService.GetCashierDailyBalanceAsync(cashierEvent.Id, model.Day.Date);


            if (entity == null)
            {
                entity = new CashierDailyBalance()
                {
                    CashierEventId = cashierEvent.Id,
                    Day = model.Day.Date,
                    OpeningFundBalanceAmount = model.OpeningFundBalanceAmount
                };

                await _cashierEventService.InsertCashierDailyBalanceAsync(entity);
            }
            else
            {
                entity.OpeningFundBalanceAmount = model.OpeningFundBalanceAmount;
                await _cashierEventService.UpdateCashierDailyBalanceAsync(entity);
            }


            return Ok();
        }


        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> EditDailyFundBalanceAsync(CashierDailyBalanceItemModel model)
        {
            var entity = await _cashierEventService.GetCashierDailyBalanceByIdAsync(model.Id);
            if (entity == null)
            {
                return Content("not found");
            }

            entity.OpeningFundBalanceAmount = model.OpeningFundBalanceAmount;

            await _cashierEventService.UpdateCashierDailyBalanceAsync(entity);

            return new NullJsonResult();
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> DeleteDailyFundBalanceAsync(CashierDailyBalanceItemModel model)
        {
            var entity = await _cashierEventService.GetCashierDailyBalanceByIdAsync(model.Id);
            if (entity == null)
            {
                return Content("not found");
            }
            await _cashierEventService.DeleteCashierDailyBalanceAsync(entity);

            return new NullJsonResult();
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ListDailyBalances(CashierDailyBalanceSearchModel searchModel)
        {
            var model = await _cashierEventAdminFactory.PrepareEventCashierDailyBalanceListModel(searchModel);
            return Json(model);
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ListAsync(EventCashierEventsViewModel viewModel)
        {
            var model = await _cashierEventAdminFactory.PrepareEventCashierEventsListModelAsync(viewModel);
            return Json(model);
        }


        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ListCashiersAsync(int eventId, CustomerSearchModel searchModel)
        {
            var model = await _cashierEventAdminFactory.PrepareCashierListModelAsync(eventId, searchModel);
            return Json(model);
        }


        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> AddEventCashierEventAsync([FromForm] AddCashierToCashierEventsModel model)
        {
            var previousRecords = await _cashierEventService.GetEventCashiersAsync(model.EventId, model.SelectedCustomerIds.ToList());
            if (previousRecords.Any())
            {
                throw new NopException($"Cashiers with ids {string.Join(",", previousRecords.Select(ae => ae.CustomerId))} are already added.");
            }


            var cashierEvents = model.SelectedCustomerIds.Select(customerId => new CashierEvent
            {
                CustomerId = customerId,
                EventId = model.EventId,
                Active = true,
            }).ToList();


            await _cashierEventService.InsertAsync(cashierEvents);
            return Ok();
        }


        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            var cashierEvent = await _cashierEventService.GetByIdAsync(id);
            if (cashierEvent == null)
            {
                throw new NopException("CashierEvent not found");
            }

            await _cashierEventService.DeleteAsync(cashierEvent);
            return new NullJsonResult();
        }


        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> EditAsync(CashierEventsItemModel model)
        {
            var cashierEvent = await _cashierEventService.GetByIdAsync(model.Id);
            if (cashierEvent == null)
            {
                return Content("CashierEvent not found");
            }

            cashierEvent.Active = model.Active;
            cashierEvent.IsRefundPermitted = model.IsRefundPermitted;

            await _cashierEventService.UpdateAsync(cashierEvent);

            return new NullJsonResult();
        }

        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public IActionResult ChangeBalance(int cashierEventId)
        {
            var model = new ChangeCashierBalanceViewModel()
            {
                CashierEventId = cashierEventId,
                AvailableChangeTypes = Enum
                    .GetValues<AdminChangeBalanceType>()
                    .Select(type => new SelectListItem
                    {
                        Text = type.ToString(),
                        Value = ((int)type).ToString()
                    })
                    .ToList()
            };
            return View("ChangeBalancePopup", model);
        }


        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> ChangeBalanceAsync(ChangeCashierBalanceViewModel model)
        {
            if (model.CashierEventId <= 0)
            {
                _notificationService.ErrorNotification("Invalid Id");
                return RedirectToAction("ChangeBalance");
            }

            var request = new CashierBalanceTransactionRequest()
            {
                CashierEventId = model.CashierEventId,
                Amount = model.ChangeAmount,
                Type = model.ChangeType == (int)AdminChangeBalanceType.Increase
                    ? CashierBalanceHistoryType.AdminIncreased
                    : CashierBalanceHistoryType.AdminDeducted,
                Note = "Admin Changed Balance"
            };
            var canPerform = await _cashierBalanceService.CanPerformAsync(request);
            if (canPerform == false)
            {
                _notificationService.ErrorNotification("Failed to perform admin change balance");
                return RedirectToAction("ChangeBalance");
            }

            await _cashierBalanceService.PerformAsync(request);

            return Ok();
        }
    }
}