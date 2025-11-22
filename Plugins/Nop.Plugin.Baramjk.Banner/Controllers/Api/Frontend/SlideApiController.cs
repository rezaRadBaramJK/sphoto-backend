using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Banner.Domain.AnywhereSliders;
using Nop.Plugin.Baramjk.Banner.Services;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;

namespace Nop.Plugin.Baramjk.Banner.Controllers.Api.Frontend
{
    public class SlideApiController : BaseBaramjkApiController
    {
        private readonly SliderService _sliderService;

        public SlideApiController(SliderService sliderService)
        {
            _sliderService = sliderService;
        }

        [HttpPost("/api-frontend/Slide/")]
        [HttpPost("/FrontendApi/Slide/")]
        public async Task<IActionResult> AddAsync([FromBody] Slide model)
        {
            await _sliderService.InsertSlideAsync(model);
            return ApiResponseFactory.Success(model);
        }

        [HttpPut("/api-frontend/Slide/")]
        [HttpPut("/FrontendApi/Slide/")]
        public async Task<IActionResult> EditAsync([FromBody] Slide model)
        {
            await _sliderService.UpdateSlideAsync(model);
            return ApiResponseFactory.Success(model);
        }

        [HttpDelete("/api-frontend/Slide/{id}")]
        [HttpDelete("/FrontendApi/Slide/{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            var slider = await _sliderService.GetSlideByIdAsync(id);
            await _sliderService.DeleteSlideAsync(slider);
            return ApiResponseFactory.Success(slider);
        }

        [HttpGet("/api-frontend/Slide/list")]
        [HttpGet("/FrontendApi/Slide/list")]
        public async Task<IActionResult> List()
        {
            var banners = await _sliderService.GetAllSlidesAsync();
            return ApiResponseFactory.Success(banners);
        }
    }
}