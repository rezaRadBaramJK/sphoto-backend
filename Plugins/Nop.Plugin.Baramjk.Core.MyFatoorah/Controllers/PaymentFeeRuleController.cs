using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains.PaymentFeeRule;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Extensions;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Factories;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models.PaymentFeeRule;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Controllers
{
    [Area(AreaNames.Admin)]
    [Route("Admin/MyFatoorah/[controller]/[action]")]
    public class PaymentFeeRuleController : BaseBaramjkPluginController
    {
        private readonly PaymentFeeRuleFactory _paymentFeeRuleFactory;
        private readonly PaymentFeeRuleService _paymentFeeRuleService;
        private readonly INotificationService _notificationService;


        public PaymentFeeRuleController(PaymentFeeRuleFactory paymentFeeRuleFactory,
            PaymentFeeRuleService paymentFeeRuleService,
            INotificationService notificationService)
        {
            _paymentFeeRuleFactory = paymentFeeRuleFactory;
            _paymentFeeRuleService = paymentFeeRuleService;
            _notificationService = notificationService;
        }

        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Views/{ControllerName}/{viewName}.cshtml";
        }

        public IActionResult List()
        {
            var model = _paymentFeeRuleFactory.PrepareSearchModel();
            return View("List", model);
        }


        [HttpPost]
        public async Task<IActionResult> ListAsync(PaymentFeeRuleSearchModel searchModel)
        {
            var model = await _paymentFeeRuleFactory.PrepareListModelAsync(searchModel);
            return Json(model);
        }


        [HttpGet]
        public async Task<IActionResult> CreateAsync()
        {
            var viewModel = await _paymentFeeRuleFactory.PrepareViewModelAsync();
            return View("Create", viewModel);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> EditAsync(int id)
        {
            var paymentFeeRule = await _paymentFeeRuleService.GetByIdAsync(id);
            if (paymentFeeRule == null)
            {
                _notificationService.ErrorNotification("PaymentRule not found.");
                return RedirectToAction("List");
            }

            var viewModel = await _paymentFeeRuleFactory.PrepareViewModelAsync(paymentFeeRule);
            return View("Edit", viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var paymentFeeRule = await _paymentFeeRuleService.GetByIdAsync(id);
            if (paymentFeeRule == null)
            {
                _notificationService.ErrorNotification("PaymentRule not found.");
                return Content("PaymentRule not found.");
            }

            await _paymentFeeRuleService.DeleteAsync(paymentFeeRule);
            return new NullJsonResult();
        }

        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateAsync(PaymentFeeRuleViewModel viewModel, bool continueEditing)
        {
            if (viewModel.PaymentMethodId == 0 || viewModel.CountryId == 0)
            {
                _notificationService.ErrorNotification("Invalid Country or Payment Method");
                return RedirectToAction("Create");
            }

            var prevRule = await _paymentFeeRuleService.GetByMethodIdAndCountryId(viewModel.PaymentMethodId, viewModel.CountryId);
            if (prevRule != null)
            {
                _notificationService.ErrorNotification("There is already a Payment fee Rule for this country and Payment Method.");
                return RedirectToAction("Create");
            }

            var record = viewModel.Map<PaymentFeeRule>();

            await _paymentFeeRuleService.InsertAsync(record);
            return continueEditing
                ? RedirectToAction("Edit", new { id = record.Id })
                : RedirectToAction("List");
        }

        [HttpPost("{id:int}")]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditAsync(int id, PaymentFeeRuleViewModel viewModel, bool continueEditing)
        {
            if (id <1 || viewModel.PaymentMethodId == 0 || viewModel.CountryId == 0)
            {
                _notificationService.ErrorNotification("Invalid Country or Payment Method");
                return RedirectToAction("Create");
            }

            var prevRule = await _paymentFeeRuleService.GetByIdAsync(id);
            if (prevRule == null)
            {
                _notificationService.ErrorNotification("Invalid paymentRuleId");
                return RedirectToAction("Create");
            }

            prevRule = viewModel.Map<PaymentFeeRule>();

            await _paymentFeeRuleService.UpdateAsync(prevRule);
            return continueEditing
                ? RedirectToAction("Edit", new { id = prevRule.Id })
                : RedirectToAction("List");
        }
    }
}