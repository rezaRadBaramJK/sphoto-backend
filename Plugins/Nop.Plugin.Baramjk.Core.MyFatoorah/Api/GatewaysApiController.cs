using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Factories;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Api
{
    public class GatewaysApiController: BaseBaramjkApiController
    {
        private readonly MyFatoorahPaymentClient _myFatoorahPaymentClient;
        private readonly GatewayFactory _gatewayFactory;

        public GatewaysApiController(
            MyFatoorahPaymentClient myFatoorahPaymentClient, 
            GatewayFactory gatewayFactory)
        {
            _myFatoorahPaymentClient = myFatoorahPaymentClient;
            _gatewayFactory = gatewayFactory;
        }

        [HttpGet("/FrontendApi/MyFatoorah/Gateways")]
        public async Task<IActionResult> GetAvailablePaymentGatewaysAsync()
        {
            try
            {
                var initiatePaymentRequest = new InitiatePaymentRequest
                {
                    CurrencyIso = "KWD",
                    InvoiceAmount = 100
                };
                var methods = await _myFatoorahPaymentClient.GetPaymentMethodsAsync(initiatePaymentRequest);
                var results = await _gatewayFactory.PrepareApiResultAsync(methods);
                return ApiResponseFactory.Success(results);

            }
            catch (Exception e)
            {
                return ApiResponseFactory.InternalServerError(e.Message);
            }
            
        }
    }
}