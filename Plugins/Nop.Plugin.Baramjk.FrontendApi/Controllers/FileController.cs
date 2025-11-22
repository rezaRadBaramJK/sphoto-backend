using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Media;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Services;
using Nop.Services.Media;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class FileController : BaseNopWebApiFrontendController
    {
        private readonly IDownloadService _downloadService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly FrontendFileService _frontendFileService;

        public FileController(
            IDownloadService downloadService,
            IWebHostEnvironment hostingEnvironment,
            FrontendFileService frontendFileService)
        {
            _downloadService = downloadService;
            _hostingEnvironment = hostingEnvironment;
            _frontendFileService = frontendFileService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFileAsync([FromForm] IFormFile file)
        {
            var validationResult = _frontendFileService.ValidateFile(file);
            
            if (validationResult.IsValid == false)
                return ApiResponseFactory.BadRequest(validationResult.Message);
            
            var fileBinary = await _downloadService.GetDownloadBitsAsync(file);
            var fileName = file.FileName;
            var contentType = file.ContentType;
            var fileExtension = Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = string.Empty,
                DownloadBinary = fileBinary,
                ContentType = contentType,
                Filename = Path.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            await _downloadService.InsertDownloadAsync(download);

            var data = new
            {
                download.Id,
                Guid = download.DownloadGuid.ToString(),
                Url = "File/GetFileUpload/" + download.DownloadGuid
            };

            return ApiResponseFactory.Success(data);
        }

        [HttpGet("/frontendApi/File/GetFileUpload/{downloadId}")]
        [HttpGet("/File/GetFileUpload/{downloadId}")]
        public virtual async Task<IActionResult> GetFileUpload([FromRoute] Guid downloadId)
        {
            var download = await _downloadService.GetDownloadByGuidAsync(downloadId);
            if (download == null)
                return ApiResponseFactory.BadRequest("Download is not available any more.");

            if (download.DownloadBinary == null)
                return ApiResponseFactory.BadRequest("Download data is not available any more.");

            var fileName = $"{download.DownloadGuid}{download.Extension}";
            var folderPath = Path.Combine("wwwroot", "files", "Files");
            var fullPath = Path.Combine(_hostingEnvironment.ContentRootPath, folderPath, fileName);
            if (System.IO.File.Exists(fullPath))
                return Redirect($"/files/Files/{fileName}");

            Directory.CreateDirectory(Path.Combine(_hostingEnvironment.ContentRootPath, folderPath));
            await using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
            {
                fs.Write(download.DownloadBinary, 0, download.DownloadBinary.Length);
            }

            return Redirect($"/files/Files/{fileName}");
        }
    }
}