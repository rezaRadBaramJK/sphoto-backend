using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Baramjk.Banner.Domain;
using Nop.Plugin.Baramjk.Banner.Models;
using Nop.Plugin.Baramjk.Banner.Services;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Services.Customers;
using Nop.Services.Media;

namespace Nop.Plugin.Baramjk.Banner.Controllers.Api.Frontend
{
    public class GifMetaData
    {
        public double Duration { get; set; }
        public double Fps { get; set; }
    }
    public class BannerApiController : BaseBaramjkApiController
    {
        private readonly BannerService _bannerService;
        private readonly IDownloadService _downloadService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly BannerLocalizationService _bannerLocalizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        
        public BannerApiController(
            BannerService bannerService,
            IDownloadService downloadService,
            IWebHostEnvironment hostingEnvironment,
            BannerLocalizationService bannerLocalizationService,
            IHttpContextAccessor httpContextAccessor,
            IWorkContext workContext,
            ICustomerService customerService)
        {
            _bannerService = bannerService;
            _downloadService = downloadService;
            _hostingEnvironment = hostingEnvironment;
            _bannerLocalizationService = bannerLocalizationService;
            _httpContextAccessor = httpContextAccessor;
            _workContext = workContext;
            _customerService = customerService;
        }

        [HttpGet("/api/banner/list")]
        [HttpGet("/api-frontend/banner/list")]
        [HttpGet("/FrontendApi/banner/list")]
        public async Task<IActionResult> BannerList(int? categoryId = null, int? productId = null,
            int? vendorId = null, BannerType? bannerType = null, string tag = "", int? entityId = null,
            string entityName = null, string entityIds = null, bool includeSvgTag = false)
        {
            if (categoryId != null)
            {
                entityId = categoryId;
                entityName = "Category";
            }
            else if (productId != null)
            {
                entityId = productId;
                entityName = "Product";
            }
            else if (vendorId != null)
            {
                entityId = vendorId;
                entityName = "Vendor";
            }

            var ints = string.IsNullOrEmpty(entityIds)
                ? null
                : entityIds.Split(",").Select(item => (int?)int.Parse(item)).ToList();
            var banners = await _bannerService.GetBannersAsync(entityName, entityId, bannerType, tag, ints);

            if (string.IsNullOrEmpty(tag) && includeSvgTag == false)
            {
                for (var i = banners.Count - 1; i >= 0;i--)
                {
                    var b = banners[i];
                    if(b.Tag != "SvgIcon")
                        continue;
                    banners.Remove(b);
                }
            }

            await _bannerLocalizationService.UpdateLocalizationAsync(banners);
            
            var bannerModels = banners.Select(item => new BannerModelDto
            {
                Id = item.Id,
                FileName = item.FileName,
                Title = item.Title,
                Text = item.Text,
                Link = item.Link,
                AltText = item.AltText,
                Tag = item.Tag,
                DisplayOrder = item.DisplayOrder,
                BannerType = item.BannerType,
                EntityId = item.EntityId,
                EntityName = item.EntityName,
                FileUrl = $"banner/GetFile/{item.FileName}",
                ExpirationDateTime = item.ExpirationDateTime
                // MetaData = await GetGifMetaData(item)
            })
                .OrderBy(b => b.DisplayOrder)
            .ToList();

            return ApiResponseFactory.Success(bannerModels);
        }

        private async Task<GifMetaData> GetGifMetaData(BannerRecord record)
        {
            if (record.BannerType!=BannerType.Gif)
            {
                return default;
            }
            var download = await _downloadService.GetDownloadByGuidAsync(Guid.Parse(record.FileName));
            if (download == null)
                return default;
          
            var fileName = $"{download.DownloadGuid}{download.Extension}";
            var folderPath = Path.Combine("wwwroot", "files", "banner");
            var fullPath = Path.Combine(_hostingEnvironment.ContentRootPath, folderPath, fileName);
            var minimumFrameDelay = (1000.0/60);

            using (Image gifImg = Image.FromFile(fullPath))
            {
                var dimension = new FrameDimension(gifImg.FrameDimensionsList[0]); 
                int frameCount = gifImg.GetFrameCount(dimension);
                double totalDuration = 0;

                for (int i = 0; i < frameCount; i++)
                {
                    var delayPropertyBytes = gifImg.GetPropertyItem(20736).Value;
                    Console.WriteLine(JsonConvert.SerializeObject(delayPropertyBytes));
                    var frameDelay = BitConverter.ToInt32(delayPropertyBytes, i * 4) * 10;

                    // Minimum delay is 16 ms. It's 1/60 sec i.e. 60 fps
                    totalDuration += (frameDelay < minimumFrameDelay ? (int) minimumFrameDelay : frameDelay);

                }

                double totalDurationInSeconds = totalDuration / 1000.0;
                double fps = frameCount / totalDurationInSeconds;

                
                return new GifMetaData
                {
                    Duration = totalDuration,
                    Fps = fps
                };
            }
        }
        
        
        [HttpGet("/api-frontend/banner/GetFile/{downloadId}")]
        [HttpGet("/FrontendApi/banner/GetFile/{downloadId}")]
        [HttpGet("/banner/GetFile/{downloadId}")]
        public virtual async Task<IActionResult> GetFileUpload([FromRoute] Guid downloadId)
        {
            var queries = _httpContextAccessor.HttpContext.Request.QueryString.Value.TrimStart('?');

            var download = await _downloadService.GetDownloadByGuidAsync(downloadId);
            if (download == null)
                return ApiResponseFactory.BadRequest("Download is not available any more.");

            if (download.DownloadBinary == null)
                return ApiResponseFactory.BadRequest("Download data is not available any more.");

            var fileName = $"{download.DownloadGuid}{download.Extension}";
            var folderPath = Path.Combine("wwwroot", "files", "banner");
            var fullPath = Path.Combine(_hostingEnvironment.ContentRootPath, folderPath, fileName);
            if (System.IO.File.Exists(fullPath))
                return Redirect($"/files/banner/{fileName}?{queries}");

            Directory.CreateDirectory(Path.Combine(_hostingEnvironment.ContentRootPath, folderPath));
            await using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
            {
                await fs.WriteAsync(download.DownloadBinary.AsMemory(0, download.DownloadBinary.Length));
            }

            return Redirect($"/files/banner/{fileName}?{queries}");
        }

        [HttpPost("/api-frontend/banner/")]
        [HttpPost("/FrontendApi/banner/")]
        public async Task<IActionResult> AddBannerApi([FromForm] BannerModel model)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsAdminAsync(currentCustomer) == false)
                return ApiResponseFactory.Unauthorized("Access denied.");
            
            await _bannerService.AddAsync(model);
            return ApiResponseFactory.Success();
        }

        [HttpPut("/api-frontend/banner/")]
        [HttpPut("/FrontendApi/banner/")]
        public async Task<IActionResult> EditBannerApi([FromForm] BannerModel model)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsAdminAsync(currentCustomer) == false)
                return ApiResponseFactory.Unauthorized("Access denied.");
            
            await _bannerService.EditAsync(model);
            return ApiResponseFactory.Success();
        }

        [HttpDelete("/api-frontend/banner/{id}")]
        [HttpDelete("/FrontendApi/banner/{id}")]
        public async Task<IActionResult> DeleteBanner([FromRoute] int id)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsAdminAsync(currentCustomer) == false)
                return ApiResponseFactory.Unauthorized("Access denied.");
            
            await _bannerService.DeleteAsync(id);
            var referer = Request.Headers["Referer"];
            if (string.IsNullOrEmpty(referer) == false)
                return Redirect(referer);

            return ApiResponseFactory.Success();
        }

        [HttpGet("/api-frontend/banner/GetTags")]
        [HttpGet("/FrontendApi/banner/GetTags")]
        public async Task<IActionResult> GetTagsAsync([FromRoute] int id)
        {
            var bannerTags = await _bannerService.GetTagsAsync();
            return ApiResponseFactory.Success(bannerTags);
        }
    }
}