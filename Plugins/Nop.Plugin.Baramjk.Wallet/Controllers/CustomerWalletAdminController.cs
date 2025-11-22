using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Models.Api;
using Nop.Plugin.Baramjk.Wallet.Plugins;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Services.Directory;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.Wallet.Controllers
{
    [Permission(PermissionProvider.Wallet_MANAGEMEN)]
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    public class CustomerWalletAdminController : BaseBaramjkApiController
    {
        private readonly IInternalWalletService _internalWalletService;
        private readonly IWalletService _walletService;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        

        public CustomerWalletAdminController(
            ICurrencyService currencyService, IWorkContext workContext, IInternalWalletService internalWalletService, IWalletService walletService)
        {
            _currencyService = currencyService;
            _workContext = workContext;
            _internalWalletService = internalWalletService;
            _walletService = walletService;
        }

        [HttpPost("/Admin/Wallet/Amount")]
        public async Task<IActionResult> ChargeCustomerAmountAsync([FromBody] ChargeCustomerApiParams apiParams)
        {
            var validationResult = Validation(apiParams);
            if (string.IsNullOrEmpty(validationResult) == false)
                return BadRequest(validationResult);

            var currency = await _currencyService.GetCurrencyByIdAsync(apiParams.CurrencyId);
            if (currency == null)
                return BadRequest("invalid currency code.");
            
            var wallet = await _internalWalletService.GetWalletAsync(apiParams.CustomerId, currency.CurrencyCode);
            if (wallet == null)
                return BadRequest("wallet not found.");
            
            if (apiParams.AmountToUpdate == 0)
                return Ok();

            var selectedHistoryType = (WalletHistoryType)apiParams.HistoryTypeId;
            if (selectedHistoryType != WalletHistoryType.Reward && selectedHistoryType != WalletHistoryType.Withdrawal)
                return BadRequest("invalid history type.");

            if (selectedHistoryType == WalletHistoryType.Withdrawal)
            {
                // apiParams.AmountToUpdate *= -1;
                apiParams.ExpirationDate = string.Empty;
            }
            // todo : remove this?
            var hasExpirationDate = DateTime.TryParse(apiParams.ExpirationDate, out var expirationDate);
            var admin = await _workContext.GetCurrentCustomerAsync();
            try
            {
                await _walletService.PerformAsync(
                    new WalletTransactionRequest
                    {
                        CustomerId = apiParams.CustomerId,
                        CurrencyCode = currency.CurrencyCode,
                        Amount = apiParams.AmountToUpdate,
                        Type = (WalletHistoryType)apiParams.HistoryTypeId,
                        ExpireDateTime = hasExpirationDate ? expirationDate : default,
                        TxId = Guid.NewGuid(),
                        Note = $"add history from admin={admin.Id} type:{(WalletHistoryType)apiParams.HistoryTypeId} amount : {apiParams.AmountToUpdate}",
                        ExtraData = $"add history from admin={admin.Id} type:{(WalletHistoryType)apiParams.HistoryTypeId} amount : {apiParams.AmountToUpdate}",
                    }
                );
                //todo add expr
                // await _walletService.PerformAsync(
                //     apiParams.CustomerId,
                //     currency.CurrencyCode,
                //     apiParams.AmountToUpdate,
                //     (WalletHistoryType)apiParams.HistoryTypeId,
                //     0,
                //     0,
                //     $"updated by {admin.Email}"
                //     // 0
                //     // hasExpirationDate ? expirationDate : null
                //     );
                
                return Ok();
            }
            catch (BadRequestBusinessException e)
            {
                return BadRequest(e.Message);
            }
        }

        private string Validation(ChargeCustomerApiParams apiParams)
        {
            if (apiParams.CustomerId <= 0)
                return "invalid customer id. please fix it and try again.";

            if (apiParams.AmountToUpdate < 0)
                return "invalid amount. please fix it and try again.";

            if (apiParams.CurrencyId <= 0)
                return "invalid currency id. please fix it and try again.";
            
            return string.Empty;
        }

        

    }
}