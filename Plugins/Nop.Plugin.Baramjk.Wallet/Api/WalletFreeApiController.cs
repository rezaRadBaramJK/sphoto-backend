using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Services;

namespace Nop.Plugin.Baramjk.Wallet.Api
{
    public class WalletFreeApiController : BaseBaramjkApiController
    {
        private readonly IWalletService _walletService;
        private readonly IWorkContext _workContext;
        private readonly IInternalWalletService _internalWalletService;
        public WalletFreeApiController(IWalletService walletService, IWorkContext workContext, IInternalWalletService internalWalletService)
        {
            _walletService = walletService;
            _workContext = workContext;
            _internalWalletService = internalWalletService;
        }


        [HttpPost("/api-frontend/wallet/freeCoin")]
        [HttpPost("/frontendApi/wallet/freeCoin")]
        public async Task<IActionResult> FreeCoin([FromBody] FreeCoinRequest request)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _walletService.PerformAsync(new WalletTransactionRequest
            {
                CustomerId = customer.Id,
                Amount = 1,
                CurrencyCode = "DMD",
                Type = WalletHistoryType.FreeAd,
                Note = "free add reward"
            });
            var walletModels = await _internalWalletService.GetCustomerWalletsAsync(customer.Id);
            return ApiResponseFactory.Success(walletModels);
        }

        public class FreeCoinRequest
        {
            public string RequestType { get; set; }
            public string Provider { get; set; }
            public string ReferenceCode { get; set; }
        }
    }
}