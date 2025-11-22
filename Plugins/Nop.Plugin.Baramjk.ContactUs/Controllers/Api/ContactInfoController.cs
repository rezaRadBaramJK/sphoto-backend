using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Baramjk.ContactUs.Domains;
using Nop.Plugin.Baramjk.ContactUs.Models.Api.ContactInfos;
using Nop.Plugin.Baramjk.ContactUs.Models.Services.Payments;
using Nop.Plugin.Baramjk.ContactUs.Services;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Services.Directory;
using Nop.Services.Media;

namespace Nop.Plugin.Baramjk.ContactUs.Controllers.Api
{
    public class ContactInfoController: BaseBaramjkApiController
    {
        private readonly ICountryService _countryService;
        private readonly SubjectService _subjectService;
        private readonly ContactInfoService _contactInfoService;
        private readonly IDownloadService _downloadService;
        private readonly ContactUsPaymentService _contactUsPaymentService;
        
        public ContactInfoController(
            ICountryService countryService,
            SubjectService subjectService,
            ContactInfoService contactInfoService,
            IDownloadService downloadService,
            ContactUsPaymentService contactUsPaymentService)
        {
            _countryService = countryService;
            _subjectService = subjectService;
            _contactInfoService = contactInfoService;
            _downloadService = downloadService;
            _contactUsPaymentService = contactUsPaymentService;
        }


        [HttpPost("/FrontendApi/ContactUs/ContactInfo")]
        public async Task<IActionResult> SubmitAsync([FromBody] SubmitContactInfoApiParams apiParams)
        {
            if (string.IsNullOrEmpty(apiParams.FirstName))
                return ApiResponseFactory.BadRequest("Invalid first name.");
            
            if (string.IsNullOrEmpty(apiParams.LastName))
                return ApiResponseFactory.BadRequest("Invalid last name.");
            
            if (string.IsNullOrEmpty(apiParams.PhoneNumber))
                return ApiResponseFactory.BadRequest("Invalid phone number.");
            
            if (string.IsNullOrEmpty(apiParams.Email))
                return ApiResponseFactory.BadRequest("Invalid email.");

            var country = await _countryService.GetCountryByIdAsync(apiParams.CountryId);
            if (country == null)
                return ApiResponseFactory.BadRequest("Invalid country.");

            SubjectEntity subject = null;
            if (apiParams.SubjectId > 0)
            {
                subject = await _subjectService.GetByIdAsync(apiParams.SubjectId);
                if (subject == null || subject.Deleted)
                    return ApiResponseFactory.BadRequest("Invalid subject.");
                
            }
            
            var entity = AutoMapperConfiguration.Mapper.Map<ContactInfoEntity>(apiParams);
            
            if (string.IsNullOrEmpty(apiParams.FileGuid) == false)
            {
                var download = await _downloadService.GetDownloadByGuidAsync(new Guid(apiParams.FileGuid));
                if (download == null)
                    return ApiResponseFactory.BadRequest("Invalid file.");

                entity.FileId = download.Id;
            }

            await _contactInfoService.InsertAsync(entity);
            var apiResults = new SubmitContactInfoApiResults();
            
            if (subject is { Price: > 0 })
            {
                apiResults.DoseNeedToPay = true;
                apiResults.PaymentUrl = await _contactUsPaymentService.GetPaymentUrlAsync(new GetPaymentUrlServiceParams
                {
                    ContactInfoId = entity.Id,
                    Price = subject.Price,
                    CustomerFirstName = apiParams.FirstName,
                    CustomerLastName = apiParams.LastName,
                    CustomerPhoneNumber = apiParams.PhoneNumber,
                    CustomerEmail = apiParams.Email,
                });;
            }
            
            return ApiResponseFactory.Success(apiResults);
        }
        
    }
}