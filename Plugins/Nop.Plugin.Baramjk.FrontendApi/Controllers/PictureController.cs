using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Media;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Services.Media;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class PictureController : BaseNopWebApiFrontendController
    {
        private readonly IDownloadService _downloadService;
        private readonly MediaSettings _mediaSettings;
        private readonly IPictureService _pictureService;

        public PictureController(IDownloadService downloadService, IPictureService pictureService,
            MediaSettings mediaSettings)
        {
            _downloadService = downloadService;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
        }

        [HttpPost]
        public async Task<IActionResult> UploadPicture(IFormFile file)
        {
            if (file == null || file.Length < 1)
                return ApiResponseFactory.BadRequest("File is empty");

            var bytes = await _downloadService.GetDownloadBitsAsync(file);
            var picture = await _pictureService.InsertPictureAsync(bytes, file.ContentType, null);
            var url = await _pictureService.GetPictureUrlAsync(picture.Id);

            var data = new
            {
                picture.Id,
                Url = url
            };

            return ApiResponseFactory.Success(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPicture([FromRoute] int id)
        {
            var pictureSize = _mediaSettings.ProductDetailsPictureSize;
            var picture = await _pictureService.GetPictureByIdAsync(id);
            string fullSizeImageUrl, imageUrl;
            (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
            (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

            var pictureModel = new PictureModel
            {
                ImageUrl = imageUrl,
                FullSizeImageUrl = fullSizeImageUrl,
                Title = picture.TitleAttribute,
                AlternateText = picture.AltAttribute
            };

            return ApiResponseFactory.Success(pictureModel);
        }
    }
}