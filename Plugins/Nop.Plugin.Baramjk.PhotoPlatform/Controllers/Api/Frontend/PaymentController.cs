using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Settings;
using Nop.Plugin.Baramjk.PhotoPlatform.Factories.ContactUs;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services.ContactUs;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Frontend
{
    public class PaymentController : BaseBaramjkApiController
    {
        private readonly PhotoPlatformContactUsPaymentService _contactUsPaymentService;
        private readonly PaymentFactory _paymentFactory;
        private readonly ITranslationService _translationService;
        private readonly PhotoPlatformContactInfoService _contactInfoService;
        private readonly ILogger _logger;
        private readonly PhotoPlatformContactUsSettings _contactUsSettings;
        private readonly PhotoPlatformContactUsNotifyService _contactUsNotifyService;


        public PaymentController(
            PhotoPlatformContactUsPaymentService contactUsPaymentService,
            PaymentFactory paymentFactory,
            PhotoPlatformContactInfoService contactInfoService,
            ILogger logger,
            ITranslationService translationService,
            PhotoPlatformContactUsSettings contactUsSettings, 
            PhotoPlatformContactUsNotifyService contactUsNotifyService)
        {
            _contactUsPaymentService = contactUsPaymentService;
            _paymentFactory = paymentFactory;
            _contactInfoService = contactInfoService;
            _logger = logger;
            _translationService = translationService;
            _contactUsSettings = contactUsSettings;
            _contactUsNotifyService = contactUsNotifyService;
        }


        [HttpGet("/FrontendApi/PhotoPlatform/ContactUs/Payment/Verify")]
        public async Task<IActionResult> VerifyAsync([FromQuery] string guid)
        {
            var redirectUrl =
                _contactUsSettings.PaymentCallbackUrl.Replace(DefaultValues.PaymentCallbackUrlPaymentIdName, guid);
            
            var translation = await _translationService.GetByGuidAsync(guid);
            if (translation == null)
                return string.IsNullOrEmpty(redirectUrl)
                    ? ApiResponseFactory.Success()
                    : Redirect(redirectUrl);

            if (translation.Status != GatewayPaymentStatus.Paid)
                return string.IsNullOrEmpty(redirectUrl)
                    ? ApiResponseFactory.Success()
                    : Redirect(redirectUrl);

            var contactInfo = await _contactInfoService.GetByIdAsync(translation.ConsumerEntityId);
            if (contactInfo == null)
            {
                var message =
                    $"Contact Us - Verify Payment: contact info with id: {translation.ConsumerEntityId} not found.";
                await _logger.ErrorAsync(message);
                return ApiResponseFactory.InternalServerError(message);
            }
            
            if (contactInfo.HasBeenPaid)
                return string.IsNullOrEmpty(redirectUrl) ? ApiResponseFactory.Success() : Redirect(redirectUrl);

            await _contactInfoService.MarkAsPaidAsync(contactInfo);

            if (_contactUsSettings.NotifyAdminAfterPayment)
            {
                await _contactUsNotifyService.SendContactOwnerPaymentSuccessfulEmailAsync(contactInfo);
            }

            return string.IsNullOrEmpty(redirectUrl)
                ? ApiResponseFactory.Success()
                : Redirect(redirectUrl);
        }
        

        [HttpGet("/FrontendApi/PhotoPlatform/ContactUs/Payment/Method")]
        public async Task<IActionResult> GetPaymentMethodsAsync([FromQuery] int countryId)
        {
            var paymentMethods = await _contactUsPaymentService.GetPaymentMethodsAsync(countryId);
            var dtoList = await _paymentFactory.PreparePaymentDtoListAsync(paymentMethods);
            return ApiResponseFactory.Success(dtoList);
        }
    }
}