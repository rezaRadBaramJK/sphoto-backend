using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Models;
using Nop.Plugin.Baramjk.Wallet.Models.Api;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Plugin.Baramjk.Wallet.Services.Models;
using Nop.Services.Common;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.Wallet.Api
{
    public class WalletApiController : BaseBaramjkApiController
    {
        private readonly WalletChargeService _walletChargeService;
        private readonly WalletPackageService _walletPackageService;
        private readonly IWalletService _walletService;
        private readonly WalletHistoryService _walletHistoryService;
        private readonly WithdrawRequestService _withdrawRequestService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IInternalWalletService _internalWalletService;
        public WalletApiController(WalletChargeService walletChargeService, WalletPackageService walletPackageService,
            IWalletService walletService, WithdrawRequestService withdrawRequestService, IWorkContext workContext, WalletHistoryService walletHistoryService, ILogger logger, IGenericAttributeService genericAttributeService, IInternalWalletService internalWalletService)
        {
            _walletChargeService = walletChargeService;
            _walletPackageService = walletPackageService;
            _walletService = walletService;
            _withdrawRequestService = withdrawRequestService;
            _workContext = workContext;
            _walletHistoryService = walletHistoryService;
            _logger = logger;
            _genericAttributeService = genericAttributeService;
            _internalWalletService = internalWalletService;
        }

        [HttpGet("/api-frontend/wallet/list")]
        [HttpGet("/frontendApi/wallet/list")]
        public async Task<IActionResult> WalletList()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var walletModels = await _internalWalletService.GetCustomerWalletsAsync(customer.Id);
            return ApiResponseFactory.Success(walletModels);
        }

        [HttpGet("/api-frontend/wallet/history/list")]
        [HttpGet("/frontendApi/wallet/history/list")]
        public async Task<IActionResult> WalletHistoryList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, string currency = "KWD", [FromQuery] bool? redeemed = null)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var walletHistoryModels = await _walletHistoryService.GetCustomerWalletHistoryAsync(customer.Id, currency, redeemed, pageNumber, pageSize);
            return ApiResponseFactory.Success(walletHistoryModels);
        }
        [HttpGet("/api-frontend/withdraw/history/list")]
        [HttpGet("/frontendApi/withdraw/history/list")]
        public async Task<IActionResult> WithdrawHistoryList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, string currency = "KWD")
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var walletHistoryModels = await _withdrawRequestService.ListAsync(customer.Id, pageNumber,pageSize,currency);
            return ApiResponseFactory.Success(walletHistoryModels);
        }

        [HttpGet("/api-frontend/wallet/package/list")]
        [HttpGet("/frontendApi/wallet/package/list")]
        public async Task<IActionResult> GetPackageModelsAsync(string currencyCode = "")
        {
            var packageModels = await _walletPackageService.GetPackageModelsAsync();
            if (string.IsNullOrEmpty(currencyCode) == false)
                packageModels = packageModels
                    .Where(item => item.Items.Any(item2 => item2.CurrencyCode.Trim() == currencyCode))
                    .ToList();

            return ApiResponseFactory.Success(packageModels);
        }

        [HttpPost("/api-frontend/wallet/charge/package/translationCode")]
        [HttpPost("/frontendApi/wallet/charge/package/translationCode")]
        public async Task<IActionResult> GetChargeTranslationCodePackage([FromQuery] int packageId, [FromQuery] decimal amount, [FromQuery] string currencyCode)
        {
            GatewayPaymentTranslation translation;
            if (packageId != 0)
            {
                translation = await _walletChargeService.CreateGatewayPaymentTranslationByPackageId(packageId);    
            }
            else if (amount != 0)
            {
                translation = await _walletChargeService.CreateGatewayPaymentTranslationByAmountAsync(amount, currencyCode);
            }
            else
            {
                return ApiResponseFactory.BadRequest("invalid packageId and amount. please enter at least one of them and try again.");
            }
            return ApiResponseFactory.Success(translation.ToResponseModel());
        }
        
        
        
        [HttpPost("/api-frontend/wallet/WithdrawRequest")]
        [HttpPost("/frontendApi/wallet/WithdrawRequest")]
        public async Task<IActionResult> AddWithdrawRequest([FromBody] WithdrawRequestModel request)
        {
            try
            {
                await _withdrawRequestService.AddAsync(request);
                return ApiResponseFactory.Success(request);
            }
            catch (InsufficientWalletBalanceException e)
            {
                return ApiResponseFactory.BadRequest("Insufficient wallet balance");
            }
            catch (CantPerformWalletTransaction e)
            {
                return ApiResponseFactory.BadRequest("Can't Perform Wallet Transaction");

            }
        }

        [HttpPost("/api-frontend/wallet/UseWalletCredit")]
        [HttpPost("/frontendApi/wallet/UseWalletCredit")]
        public async Task<IActionResult> ChangeUseWalletCredit([FromBody] UseWalletCreditModelRequest request)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            try
            {
                await _genericAttributeService.SaveAttributeAsync(customer, "UseWalletCredit",
                    request.UseWalletCredit);
                return ApiResponseFactory.Success("OK");

            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
                return ApiResponseFactory.BadRequest("FAILED");
            }
        }

        [HttpGet("/api-frontend/wallet/UseWalletCredit")]
        [HttpGet("/frontendApi/wallet/UseWalletCredit")]
        public async Task<IActionResult> GetseWalletCredit()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var value = await _genericAttributeService.GetAttributeAsync<bool>(customer, "UseWalletCredit");
            return ApiResponseFactory.Success(new UseWalletCreditModelRequest{UseWalletCredit = value});

        }
        

        [HttpGet("/api-frontend/wallet/amount")]
        [HttpGet("/frontendApi/wallet/amount")]
        public async Task<IActionResult> GetWalletAmountByCurrencyCode([FromQuery] string currencyCode = "KWD")
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var result = await _walletService.GetAvailableAmountAsync(customer.Id, currencyCode);
            return ApiResponseFactory.Success(result);
        }
        
        [HttpGet("/api-frontend/wallet/info")]
        [HttpGet("/frontendApi/wallet/info")]
        public async Task<IActionResult> GetWalletInfoByCurrencyCode([FromQuery] string currencyCode = "KWD")
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var walletInfoModel = await _internalWalletService.GetCustomerWalletInfoAsync(customer.Id, currencyCode);
            return ApiResponseFactory.Success(walletInfoModel);
        }
        
    }
}