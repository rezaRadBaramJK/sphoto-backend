using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Models.Settings;
using Nop.Plugin.Baramjk.ContactUs.Models.Api.OwnerInfo;
using Nop.Plugin.Baramjk.ContactUs.Plugin;
using Nop.Plugin.Baramjk.ContactUs.Services;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;

namespace Nop.Plugin.Baramjk.ContactUs.Controllers.Api
{
    public class OwnerInfoController: BaseBaramjkApiController
    {
        private readonly ContactUsSettings _contactUsSettings;
        private readonly ContactInfoService _contactInfoService;
        private readonly ITranslationService _translationService;
        
        public OwnerInfoController(
            ContactUsSettings contactUsSettings,
            ContactInfoService contactInfoService,
            ITranslationService translationService)
        {
            _contactUsSettings = contactUsSettings;
            _contactInfoService = contactInfoService;
            _translationService = translationService;
        }

        [HttpGet("/FrontendApi/ContactUs/Owner")]
        public async Task<IActionResult> GetOwnerInfoAsync([FromQuery]string guid)
        {
            var translation = await _translationService.GetByGuidAsync(guid);
            if(translation == null)
                return ApiResponseFactory.BadRequest("Payment info not found.");

            if (translation.ConsumerEntityType.Equals(DefaultValues.ContactUsTableName) == false)
                return ApiResponseFactory.BadRequest("Invalid payment.");
            
            var contactInfo = await _contactInfoService.GetByIdAsync(translation.ConsumerEntityId);
            if (contactInfo == null)
                return ApiResponseFactory.BadRequest("Contact info not found.");

            if (contactInfo.HasBeenPaid == false)
                return ApiResponseFactory.BadRequest("Payment not found.");

            return ApiResponseFactory.Success(new OwnerInfoApiResponse
            {
                Email = _contactUsSettings.OwnerAdminEmail,
                PhoneNumber = _contactUsSettings.OwnerPhoneNumber
            });
        }
        
    }
}