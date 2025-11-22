using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Banner.Domain.AnywhereSliders;
using Nop.Plugin.Baramjk.Banner.Services;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.Banner.Controllers.Api.Frontend
{
    public class SliderApiController : BaseBaramjkApiController
    {
        private readonly IPictureModelFactory _pictureModelFactory;
        private readonly SliderService _sliderService;

        public SliderApiController(SliderService sliderService, IPictureModelFactory pictureModelFactory)
        {
            _sliderService = sliderService;
            _pictureModelFactory = pictureModelFactory;
        }

        [HttpPost("/api-frontend/Slider/")]
        [HttpPost("/FrontendApi/Slider/")]
        public async Task<IActionResult> AddAsync([FromBody] Slider model)
        {
            await _sliderService.InsertSliderAsync(model);
            return ApiResponseFactory.Success(model);
        }

        [HttpPut("/api-frontend/Slider/")]
        [HttpPut("/FrontendApi/Slider/")]
        public async Task<IActionResult> EditAsync([FromBody] Slider model)
        {
            await _sliderService.UpdateSliderAsync(model);
            return ApiResponseFactory.Success(model);
        }

        [HttpDelete("/api-frontend/Slider/{id}")]
        [HttpDelete("/FrontendApi/Slider/{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            var slider = await _sliderService.GetSliderByIdAsync(id);
            await _sliderService.DeleteSliderAsync(slider);
            return ApiResponseFactory.Success();
        }

        [HttpGet("/api-frontend/Slider/list")]
        [HttpGet("/FrontendApi/Slider/list")]
        public async Task<IActionResult> List()
        {
            var banners = await _sliderService.GetAllSlidersAsync();
            return ApiResponseFactory.Success(banners);
        }

        [HttpGet("/api-frontend/Slider/slides")]
        [HttpGet("/FrontendApi/Slider/slides")]
        public async Task<IActionResult> Slides(int sliderId)
        {
            var slides = await _sliderService.GetAllSlidesBySliderIdAsync(sliderId);
            var slideModels = slides.Select(item => new SlideModel
            {
                Id = item.Id,
                SystemName = item.SystemName,
                Url = item.Url,
                Alt = item.Alt,
                Visible = item.Visible,
                DisplayOrder = item.DisplayOrder,
                PictureId = item.PictureId,
                MobilePictureId = item.MobilePictureId,
                Content = item.Content,
                SlideType = item.SlideType,
                SliderId = item.SliderId,
                Picture = _pictureModelFactory.PreparePictureModel(item.PictureId).Result,
                MobilePicture = _pictureModelFactory.PreparePictureModel(item.MobilePictureId).Result
            }).ToList();

            return ApiResponseFactory.Success(slideModels);
        }
    }

    public class SlideModel
    {
        public string SystemName { get; set; }

        public string Url { get; set; }

        public string Alt { get; set; }

        public bool Visible { get; set; }

        public int DisplayOrder { get; set; }

        public int PictureId { get; set; }

        public int MobilePictureId { get; set; }

        public string Content { get; set; }

        public SlideType SlideType { get; set; }

        public int SliderId { get; set; }
        public PictureModel Picture { get; set; }
        public PictureModel MobilePicture { get; set; }
        public int Id { get; set; }
    }
}