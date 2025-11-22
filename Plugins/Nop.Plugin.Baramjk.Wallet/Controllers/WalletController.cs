using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Wallet.Models;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Plugin.Baramjk.Wallet.Services.Models;
using Nop.Services.Customers;
using Nop.Services.Messages;

namespace Nop.Plugin.Baramjk.Wallet.Controllers
{
    public class WalletController : BaseBaramjkPluginController
    {
        private readonly ICustomerService _customerService;
        private readonly IWalletService _walletService;
        private readonly IWorkContext _workContext;
        private readonly WithdrawRequestService _withdrawRequestService;
        private readonly INotificationService _notificationService;
        private readonly IInternalWalletService _internalWalletService;
        public WalletController(IWorkContext workContext, IWalletService walletService,
            ICustomerService customerService, WithdrawRequestService withdrawRequestService, INotificationService notificationService, IInternalWalletService internalWalletService)
        {
            _workContext = workContext;
            _walletService = walletService;
            _customerService = customerService;
            _withdrawRequestService = withdrawRequestService;
            _notificationService = notificationService;
            _internalWalletService = internalWalletService;
        }

        [HttpGet]
        public async Task<IActionResult> InfoPage()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var isRegistered = await _customerService.IsRegisteredAsync(customer);
            if (isRegistered == false)
                return ApiResponseFactory.Unauthorized();

            var currentCustomerId = customer.Id;
            var wallets = await _internalWalletService.GetCustomerWalletsAsync(currentCustomerId);

            return View("Wallet/InfoPage.cshtml", wallets);
        }
        
        [HttpGet]
        public async Task<IActionResult> Withdraw()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var isRegistered = await _customerService.IsRegisteredAsync(customer);
            if (isRegistered == false)
                return ApiResponseFactory.Unauthorized();

            var currentCustomerId = customer.Id;
            var wallets = await _internalWalletService.GetCustomerWalletsAsync(currentCustomerId);

            return View("Wallet/Withdraw.cshtml", wallets);
        }
        [HttpPost]
        public async Task<IActionResult> AddWithdrawRequest([FromBody] WithdrawRequestModel request)
        {
            try
            {
                await _withdrawRequestService.AddAsync(request);
                return ApiResponseFactory.Success("Withdraw request submitted");
                // _notificationService.SuccessNotification("Withdraw request submitted");
            }
            catch (InsufficientWalletBalanceException e)
            {
                return ApiResponseFactory.BadRequest("Insufficient wallet balance");

                // _notificationService.ErrorNotification("Insufficient wallet balance");

            }
            catch (Exception e)
            {
                return ApiResponseFactory.BadRequest(e.Message);

            }

        }
        protected override string SystemName => DefaultValue.SystemName;
    }
}