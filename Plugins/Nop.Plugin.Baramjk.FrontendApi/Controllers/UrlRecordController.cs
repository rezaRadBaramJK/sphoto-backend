using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Seo;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Seo;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class UrlRecordController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public UrlRecordController(IUrlRecordService urlRecordService)
        {
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets a URL record by slug
        /// </summary>
        /// <param name="slug">Slug</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(UrlRecordDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetBySlug([FromQuery] [Required] string slug)
        {
            var urlRecord = await _urlRecordService.GetBySlugAsync(slug);

            if (urlRecord == null) return ApiResponseFactory.NotFound($"Delivery date by slug={slug} not found");

            return ApiResponseFactory.Success(urlRecord.ToDto<UrlRecordDto>());
        }

        #endregion
    }
}