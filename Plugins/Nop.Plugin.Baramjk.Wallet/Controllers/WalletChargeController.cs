using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.Framework.Services.Networks;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Plugins;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Services.Messages;

namespace Nop.Plugin.Baramjk.Wallet.Controllers
{
    public class WalletChargeController : BaseBaramjkPluginController
    {
        public const string ConsumerName = "Wallet";

        private readonly WalletChargeService _walletChargeService;
        private readonly IWalletService _walletService;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;

        public WalletChargeController(WalletChargeService walletChargeService, IWalletService walletService,
            IWorkContext workContext, INotificationService notificationService)
        {
            _walletChargeService = walletChargeService;
            _walletService = walletService;
            _workContext = workContext;
            _notificationService = notificationService;
        }

        [Permission(PermissionProvider.Wallet_MANAGEMEN)]
        [HttpGet]
        [HttpPost]
        [Route("walletCharge/IncreaseBalance")]
        public async Task<IActionResult> IncreaseBalance(int customerId, decimal amount, string currencyCode)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _walletService.PerformAsync(new WalletTransactionRequest
            {
                CustomerId = customerId,
                CurrencyCode = currencyCode,
                Amount = amount,
                Type = WalletHistoryType.Charge,
                Note = $"charge from admin:{customer.Id}"
            });
            return Redirect("/Admin/Customer/Edit/" + customerId);
        }

        [HttpPost]
        public async Task<IActionResult> ChargePayment(decimal amount, string currencyCode)
        {
            var response = await _walletChargeService.ChargePaymentAsync(amount, currencyCode);
            if (response.IsSuccess() == false)
            {
                _notificationService.ErrorNotification(response.GetMessage("Send invoice fail"));
                return Redirect("/");
            }

            return Redirect(response.Body.PaymentUrl);
        }

        [HttpGet]
        public async Task<IActionResult> ChargePaymentByPackageId(int packageId)
        {
            var response = await _walletChargeService.ChargePaymentByPackageIdAsync(packageId);
            if (response.IsSuccess() == false)
            {
                _notificationService.ErrorNotification(response.GetMessage("Send invoice fail"));
                return RedirectToAction("InfoPage", "Wallet", new { msg = "" });
            }

            return Redirect(response.Body.PaymentUrl);
        }

        [HttpGet]
        public async Task<IActionResult> ChargePaymentByAmount(decimal amount, string currencyCode)
        {
            var response = await _walletChargeService.ChargePaymentByAmountAsync(amount, currencyCode);
            if (response.IsSuccess() == false)
            {
                _notificationService.ErrorNotification(response.GetMessage("Send invoice fail"));
                return RedirectToAction("InfoPage", "Wallet", new { msg = "" });
            }

            return Redirect(response.Body.PaymentUrl);
        }

        [HttpGet]
        [Route("/FrontendApi/WalletCharge/ChargePaymentByAmount")]
        public async Task<IActionResult> ChargePaymentByAmountAsync(decimal amount, string currencyCode)
        {
            var response = await _walletChargeService.ChargePaymentByAmountAsync(amount, currencyCode);
            if (response.IsSuccess() == false)
            {
                return ApiResponseFactory.BadRequest(response.GetMessage("Send invoice fail"));
            }

            return ApiResponseFactory.Success(new { response.Body.PaymentUrl });
        }


        [HttpGet]
        public async Task<IActionResult> ChargePaymentCallBack(string guid)
        {
            return RedirectToAction("InfoPage", "Wallet", new { msg = "" });
        }

        protected override string SystemName => DefaultValue.SystemName;
    }
}