using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.ContactUs.Factories;
using Nop.Plugin.Baramjk.ContactUs.Services;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;

namespace Nop.Plugin.Baramjk.ContactUs.Controllers.Api
{
    public class SubjectController: BaseBaramjkApiController
    {
        private readonly SubjectService _subjectService;
        private readonly SubjectFactory _subjectFactory;

        public SubjectController(
            SubjectService subjectService,
            SubjectFactory subjectFactory)
        {
            _subjectService = subjectService;
            _subjectFactory = subjectFactory;
        }

        [HttpGet("/FrontendApi/ContactUs/Subject")]
        public async Task<IActionResult> GetAsync([FromQuery] bool? onlyPayable = null)
        {
            var entities = await _subjectService.GetAsync(onlyPayable: onlyPayable);
            var dtoList = await _subjectFactory.PrepareDtoListAsync(entities);
            return ApiResponseFactory.Success(dtoList);
        }
        
    }
}