using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Factories.ContactUs;
using Nop.Plugin.Baramjk.PhotoPlatform.Services.ContactUs;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Frontend
{
    public class SubjectController : BaseBaramjkApiController
    {
        private readonly PhotoPlatformSubjectService _subjectService;
        private readonly SubjectFactory _subjectFactory;

        public SubjectController(
            PhotoPlatformSubjectService subjectService,
            SubjectFactory subjectFactory)
        {
            _subjectService = subjectService;
            _subjectFactory = subjectFactory;
        }

        [HttpGet("/FrontendApi/PhotoPlatform/ContactUs/Subject")]
        public async Task<IActionResult> GetAsync()
        {
            var entities = await _subjectService.GetAsync();
            var dtoList = await _subjectFactory.PrepareDtoListAsync(entities);
            return ApiResponseFactory.Success(dtoList);
        }
    }
}