using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Settings;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ContactUs.OwnerInfo;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services.ContactUs;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Frontend
{
    public class OwnerInfoController: BaseBaramjkApiController
    {
        private readonly PhotoPlatformContactUsSettings _contactUsSettings;
        private readonly PhotoPlatformContactInfoService _contactInfoService;
        private readonly ITranslationService _translationService;
        
        public OwnerInfoController(
            PhotoPlatformContactUsSettings contactUsSettings,
            PhotoPlatformContactInfoService contactInfoService,
            ITranslationService translationService)
        {
            _contactUsSettings = contactUsSettings;
            _contactInfoService = contactInfoService;
            _translationService = translationService;
        }

        [HttpGet("/FrontendApi/PhotoPlatform/ContactUs/Owner")]
        public async Task<IActionResult> GetOwnerInfoAsync([FromQuery]string guid)
        {
            var translation = await _translationService.GetByGuidAsync(guid);
            if(translation == null)
                return ApiResponseFactory.BadRequest("Payment info not found.");

            if (translation.ConsumerEntityType.Equals(DefaultValues.TableNamePrefix) == false)
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