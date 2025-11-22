using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Controllers
{
    public class MyFatoorahTestController : BasePluginController
    {
        private readonly ITranslationService _translationService;
        private readonly IWorkContext _workContext;
        private readonly IGatewayClientProvider _gatewayClientProvider;
        private readonly IGatewayService _gatewayService;
        private readonly ICustomerDtoFactory _customerDtoFactory;
        private readonly IMyFatoorahTranslationVerifyPaymentService _translationVerifyPaymentService;

        public MyFatoorahTestController(ITranslationService translationService, IWorkContext workContext,
            IGatewayClientProvider gatewayClientProvider, IGatewayService gatewayService,
            ICustomerDtoFactory customerDtoFactory,
            IMyFatoorahTranslationVerifyPaymentService translationVerifyPaymentService)
        {
            _translationService = translationService;
            _workContext = workContext;
            _gatewayClientProvider = gatewayClientProvider;
            _gatewayService = gatewayService;
            _customerDtoFactory = customerDtoFactory;
            _translationVerifyPaymentService = translationVerifyPaymentService;
        }

        //Test For Plugin
        [HttpGet]
        public async Task<IActionResult> Test()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var translation = await _translationService.NewTranslationAsync(new CreateTranslationRequest
            {
                AmountToPay = 100,
                CustomerId = customer.Id,
                ConsumerName = "ConsumerName",
                ConsumerEntityType = "Order",
                ConsumerEntityId = 10,
                ConsumerCallBack = "/MyFatoorahTest/CallBack?guid={0}&state={1}",
                ConsumerData = "",
            });

            var info = await _customerDtoFactory.PrepareCustomerDtoAsync(customer.Id);
            var paymentRequest = new InvoiceRequest
            {
                FirstName = info.FirstName,
                LastName = info.LastName,
                PhoneNumber = info.Phone,
                Email = info.Email,
                Amount = 100,
            };

            var gatewayClient = _gatewayClientProvider.GetDefaultGatewayClient();
            var response = await _gatewayService.SendInvoiceAsync(gatewayClient, translation, paymentRequest);
            return Redirect(translation.PaymentUrl);
        }

        [HttpGet("/MyFatoorahTest/CallBack")]
        public async Task<IActionResult> CallBack(string guid)
        {
            var translation = await _translationService.GetByGuidAsync(guid);
            await _translationService.SetConsumerStatusAsync(guid, ConsumerStatus.Success);

            return Ok(translation);
        }

        //Test for api
        public async Task<IActionResult> CreateTranslationAsync([FromQuery] int yourEntityId = 10)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var translation = await _translationService.NewTranslationAsync(new CreateTranslationRequest
            {
                AmountToPay = 100,
                CustomerId = customer.Id,
                ConsumerName = "ConsumerName",
                ConsumerEntityType = "Order",
                ConsumerEntityId = 10,
                ConsumerCallBack = "/MyFatoorahTest/CallBack?guid={0}&state={1}",
                ConsumerData = "",
            });

            var response = translation.ToResponseModel();
            return ApiResponseFactory.Create(response);
        }

        public async Task<IActionResult> VerifyPaymentByInvoiceId([FromQuery] int invoiceId)
        {
            var translationResponse = await _translationVerifyPaymentService.VerifyByInvoiceIdAsync(invoiceId);
            if (translationResponse.IsNewSuccessPaid == false)
                return ApiResponseFactory.BadRequest(translationResponse.Message);

            var translation = translationResponse.Translation;
            //Do your business here
            await _translationService.SetConsumerStatusAsync(translation.Guid, ConsumerStatus.Success);

            return ApiResponseFactory.Success();
        }
    }
}