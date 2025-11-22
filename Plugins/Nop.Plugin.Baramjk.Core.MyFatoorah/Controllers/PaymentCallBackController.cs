using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Helpers;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models.WebHooks;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services;
using Nop.Plugin.Baramjk.Framework.Events.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;
using Nop.Services.Events;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Controllers
{
    public class PaymentCallBackController : BaseBaramjkApiController
    {
        private readonly IMyFatoorahTranslationVerifyPaymentService _translationVerifyPaymentService;
        private readonly ILogger _logger;
        private readonly MyFatoorahSettings _myFatoorahSettings;
        private readonly IWebHelper _webHelper;
        private readonly ITranslationService _translationService;
        private readonly EventPublisher _eventPublisher;

        public PaymentCallBackController(
            IMyFatoorahTranslationVerifyPaymentService translationVerifyPaymentService,
            ILogger logger,
            MyFatoorahSettings myFatoorahSettings, 
            IWebHelper webHelper, 
            ITranslationService translationService,
            EventPublisher eventPublisher)
        {
            _translationVerifyPaymentService = translationVerifyPaymentService;
            _logger = logger;
            _myFatoorahSettings = myFatoorahSettings;
            _webHelper = webHelper;
            _translationService = translationService;
            _eventPublisher = eventPublisher;
        }

        [HttpGet("/MyFatoorah/Payment/SuccessCallBack")]
        [HttpGet("/MyFatoorah/Payment/ErrorCallBack")]
        public async Task<IActionResult> CallBack(string paymentId)
        {
            var response = await _translationVerifyPaymentService.VerifyByPaymentIdAsync(paymentId);
            if (response.Translation == null)
            {
                await _logger.ErrorAsync($"MyFatoorah payment call back - Translation is null: paymentId: {paymentId}, response: {JsonConvert.SerializeObject(response)}");
                return Redirect(PrepareFailedUrl());
            }
            
            return string.IsNullOrEmpty(response.ConsumerCallBackUrl) 
                ? Redirect(_webHelper.GetStoreLocation()) 
                : Redirect(response.ConsumerCallBackUrl);
        }

        private string PrepareFailedUrl()
        {
            if (string.IsNullOrEmpty(_myFatoorahSettings.FrontendBase)) 
                return _webHelper.GetStoreLocation();
            
            var slash = _myFatoorahSettings.FrontendBase.EndsWith("/") ? string.Empty : "/";
            var failedUrl = $"{_myFatoorahSettings.FrontendBase}{slash}{_myFatoorahSettings.FailedFrontendCallback}";
            return failedUrl;
        }

        [HttpPost("/MyFatoorah/Payment/Webhooks/v2")]
        public async Task<IActionResult> WebHookAsync([FromBody] WebHookApiParams apiParams)
        {
            if (HttpContext.Request.Headers.TryGetValue(DefaultValue.WebHookSignatureHeaderName, out var signature) ==
                false)
            {
                await _logger.ErrorAsync("My Fatoorah Webhooks - Signature header not found.");
                return Unauthorized();
            }
            
            if (apiParams.Event == null || apiParams.Data == null)
                return BadRequest();

            if (apiParams.Event.Code != DefaultValue.WebHookPaymentStatusChangedCode ||
                apiParams.Event.Name != DefaultValue.WebHookPaymentStatusChangedName)
                return BadRequest();

            if (apiParams.Data.Invoice.Status != DefaultValue.WebHookPaidStatus)
                return BadRequest();

            var verifySignatureResult = SignatureHelper.VerifyPaymentStatus(apiParams, signature);
            if (verifySignatureResult.Success == false)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "My Fatoorah Webhooks - Signature validation failed.", verifySignatureResult.ErrorMessage);
                return Unauthorized();
            }

            var guid = apiParams.Data.Invoice.Reference;
            var translation = await _translationService.GetByGuidAsync(guid);
            if (translation == null)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "", "");
                return BadRequest();
            }

            if (translation.Status == GatewayPaymentStatus.Paid)
            {
                return Ok();
            }

            var paidAmound = apiParams.Data.Amount.ValueInPayCurrency;
            
            var oldTranslationStatus = translation.Status;
            var paymentStatus = GatewayPaymentStatus.Invalid;
            if (paidAmound < translation.AmountToPay)
            {
                paymentStatus = GatewayPaymentStatus.Invalid;
            }

            translation.Status = paymentStatus;
            translation.AmountPayed = paidAmound;
            translation.InvoiceId = apiParams.Data.Invoice.Id;
            await _translationService.UpdateAsync(translation);

            var statusEvent = new GatewayPaymentTranslationStatusEvent(translation, oldTranslationStatus);
            await _eventPublisher.PublishAsync(statusEvent);

            return Ok();

            // var paymentId = apiParams.Data.Transaction.PaymentId;
            // var response = await _translationVerifyPaymentService.VerifyByPaymentIdAsync(paymentId);
            // if (response.Translation == null)
            // {
            //     await _logger.ErrorAsync($"MyFatoorah Webhooks - Translation is null: paymentId: {paymentId}, response: {JsonConvert.SerializeObject(response)}");
            //     return Redirect(PrepareFailedUrl());
            // }
            //
            // return string.IsNullOrEmpty(response.ConsumerCallBackUrl) 
            //     ? Redirect(_webHelper.GetStoreLocation()) 
            //     : Redirect(response.ConsumerCallBackUrl);
        }
    }
}