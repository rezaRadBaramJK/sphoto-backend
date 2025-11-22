using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ContactUs.ContactInfos;
using Nop.Plugin.Baramjk.PhotoPlatform.Services.ContactUs;
using Nop.Services.Directory;
using Nop.Services.Media;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Frontend
{
    public class ContactInfoController : BaseBaramjkApiController
    {
        private readonly ICountryService _countryService;
        private readonly PhotoPlatformSubjectService _subjectService;
        private readonly PhotoPlatformContactInfoService _contactInfoService;
        private readonly IDownloadService _downloadService;


        public ContactInfoController(
            ICountryService countryService,
            PhotoPlatformSubjectService subjectService,
            PhotoPlatformContactInfoService contactInfoService,
            IDownloadService downloadService)
        {
            _countryService = countryService;
            _subjectService = subjectService;
            _contactInfoService = contactInfoService;
            _downloadService = downloadService;
        }


        [HttpPost("/FrontendApi/PhotoPlatform/ContactUs/ContactInfo")]
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

            if (apiParams.SubjectId > 0)
            {
                var subject = await _subjectService.GetByIdAsync(apiParams.SubjectId);
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


            return ApiResponseFactory.Success();
        }
    }
}