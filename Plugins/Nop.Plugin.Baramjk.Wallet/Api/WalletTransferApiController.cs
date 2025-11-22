using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.BarCodes;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Wallet.Models;
using Nop.Plugin.Baramjk.Wallet.Services;

namespace Nop.Plugin.Baramjk.Wallet.Api
{
    public class WalletTransferApiController : BaseBaramjkApiController
    {
        private readonly IBarCodeService _barCodeService;
        private readonly IWalletService _walletService;
        private readonly IWorkContext _workContext;

        public WalletTransferApiController(IBarCodeService barCodeService, IWalletService walletService,
            IWorkContext workContext)
        {
            _barCodeService = barCodeService;
            _walletService = walletService;
            _workContext = workContext;
        }

        [HttpGet("/api-frontend/wallet/QRCode")]
        [HttpGet("/frontendApi/wallet/QRCode")]
        public async Task<IActionResult> QRCode(bool returnUrl = true)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            // var walletModels = await _walletService.GetPrimaryCurrencyAsync(customer.Id);
            var codeModel = new QRCodeModel { Id = customer.Id };
            var walletInfo = JsonConvert.SerializeObject(codeModel);
            var qrCode = await _barCodeService.CreateQRCodeAsync(walletInfo);
            if (returnUrl)
                return ApiResponseFactory.Success(qrCode);

            return Redirect(qrCode);
        }

        [HttpPost("/api-frontend/wallet/Transfer")]
        [HttpPost("/frontendApi/wallet/Transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var result = await _walletService.TransferAsync(new WalletTransferTransactionRequest
            {
                FromCustomerId = customer.Id,
                Amount = request.Amount,
                CurrencyCode = request.CurrencyCode,
                ToCustomerId = request.ToCustomerId
            });

            return ApiResponseFactory.Success(result);
        }
    }
}