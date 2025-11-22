using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Localization;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class LanguageController : BaseNopWebApiFrontendController
    {
        private readonly ILanguageModelFactory _languageModelFactory;

        public LanguageController(ILanguageModelFactory languageModelFactory)
        {
            _languageModelFactory = languageModelFactory;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<LanguageListModel>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetLanguages()
        {
            var countries = await _languageModelFactory.PrepareLanguageListModelAsync(new LanguageSearchModel());
            return ApiResponseFactory.Success(countries);
        }
    }
}