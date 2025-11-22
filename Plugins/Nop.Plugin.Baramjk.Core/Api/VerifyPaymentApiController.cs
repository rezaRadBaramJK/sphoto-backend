using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Core.Api.Models;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Core.Api
{
    public class VerifyPaymentApiController : BaseBaramjkApiController
    {
        private readonly IGatewayClientProvider _gatewayClientProvider;
        private readonly ITranslationVerifyService _translationVerifyService;

        public VerifyPaymentApiController(IGatewayClientProvider gatewayClientProvider,
            ITranslationVerifyService translationVerifyService)
        {
            _gatewayClientProvider = gatewayClientProvider;
            _translationVerifyService = translationVerifyService;
        }

        [HttpPost("/api-frontend/Payment/VerifyByInvoiceId")]
        [HttpPost("/FrontendApi/Payment/VerifyByInvoiceId")]
        
        [HttpPost("/api-frontend/wallet/charge/package/verifyByInvoiceId")]
        [HttpPost("/frontendApi/wallet/charge/package/verifyByInvoiceId")]
        public async Task<IActionResult> VerifyByInvoiceId([FromBody] VerifyByInvoiceIdModel model)
        {
            var gatewayClient = model.Gateway == null
                ? _gatewayClientProvider.GetDefaultGatewayClient()
                : _gatewayClientProvider.GetGatewayClient(model.Gateway);

            var request = new GetInvoiceStatusRequest(model.InvoiceId);
            var verifyResult = await _translationVerifyService.VerifyAsync(gatewayClient, request);

            var responseDto = verifyResult.ToResponseModel();
            return ApiResponseFactory.Auto(responseDto.IsSuccessPaidProcess, responseDto);
        }
    }
}